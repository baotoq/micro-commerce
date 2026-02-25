using MassTransit;
using MicroCommerce.ApiService.Common.Persistence;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Replaces production dependencies with test equivalents:
/// - PostgreSQL: Testcontainers instance
/// - MassTransit: In-memory test harness
/// - Azure Blob Storage: No-op stub (image upload not tested here)
/// - Background services: Disabled (data seeders, cleanup services)
/// - Keycloak JWT: Replaced with FakeAuthenticationHandler
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public ApiWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("microcommerce_test")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all hosted services (data seeders, background cleanup)
            // These interfere with test isolation and are not needed for API tests
            System.Collections.Generic.List<ServiceDescriptor> hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();

            foreach (ServiceDescriptor service in hostedServices)
            {
                services.Remove(service);
            }

            // Remove duplicate MassTransit health check registrations.
            // AddMassTransit registers 'masstransit-bus' health check automatically.
            // AddMassTransitTestHarness below will re-register it, causing duplicate exception.
            System.Collections.Generic.List<ServiceDescriptor> masstransitHealthChecks = services
                .Where(d => d.ServiceType == typeof(HealthCheckRegistration)
                    && d.ImplementationInstance is HealthCheckRegistration hcr
                    && hcr.Name.StartsWith("masstransit", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (ServiceDescriptor registration in masstransitHealthChecks)
            {
                services.Remove(registration);
            }

            // Replace all DbContext registrations with Testcontainer connection string
            // The production app uses Aspire service discovery, but tests need direct connection

            // 1. OutboxDbContext
            services.RemoveAll<DbContextOptions<OutboxDbContext>>();
            services.AddDbContext<OutboxDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "outbox"));
            });

            // 2. CatalogDbContext
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
            });

            // 3. CartDbContext
            services.RemoveAll<DbContextOptions<CartDbContext>>();
            services.AddDbContext<CartDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cart"));
            });

            // 4. OrderingDbContext
            services.RemoveAll<DbContextOptions<OrderingDbContext>>();
            services.AddDbContext<OrderingDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"));
            });

            // 5. InventoryDbContext
            services.RemoveAll<DbContextOptions<InventoryDbContext>>();
            services.AddDbContext<InventoryDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory"));
            });

            // 6. ProfilesDbContext
            services.RemoveAll<DbContextOptions<ProfilesDbContext>>();
            services.AddDbContext<ProfilesDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "profiles"));
            });

            // 7. ReviewsDbContext
            services.RemoveAll<DbContextOptions<ReviewsDbContext>>();
            services.AddDbContext<ReviewsDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "reviews"));
            });

            // 8. WishlistsDbContext
            services.RemoveAll<DbContextOptions<WishlistsDbContext>>();
            services.AddDbContext<WishlistsDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "wishlists"));
            });

            // Replace MassTransit with in-memory test harness
            // Remove existing MassTransit registration and replace with test harness
            services.RemoveAll(typeof(IBus));
            services.RemoveAll(typeof(IBusControl));
            services.RemoveAll(typeof(IPublishEndpoint));
            services.RemoveAll(typeof(ISendEndpointProvider));

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumers(typeof(Program).Assembly);
            });

            // Replace Azure Blob Storage with no-op stub
            // Integration tests don't upload images, so we can use a null implementation
            services.RemoveAll<IImageUploadService>();
            services.AddScoped<IImageUploadService, NoOpImageUploadService>();

            // Replace Azure Avatar service with no-op stub
            services.RemoveAll<IAvatarImageService>();
            services.AddScoped<IAvatarImageService, NoOpAvatarImageService>();

            // Replace Keycloak JWT auth with fake handler for tests
            // FakeAuthenticationHandler reads X-Test-UserId header and injects claims
            services.AddAuthentication(FakeAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                    FakeAuthenticationHandler.SchemeName, options => { });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container only
        // Database schema will be created per-test-class using EnsureCreated
        // No global schema creation here — each test class handles it via IntegrationTestBase.ResetDatabase
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>
/// No-op image upload service for integration tests.
/// Tests don't upload images to Azure Blob Storage.
/// </summary>
internal class NoOpImageUploadService : IImageUploadService
{
    public Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Return a fake URL - integration tests don't actually upload images
        return Task.FromResult($"https://test.blob.core.windows.net/images/{fileName}");
    }
}

/// <summary>
/// No-op avatar image service for integration tests.
/// Tests don't process or upload avatar images to Azure Blob Storage.
/// </summary>
internal class NoOpAvatarImageService : IAvatarImageService
{
    public Task<string> ProcessAndUploadAvatarAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken ct = default)
    {
        // Return a fake avatar URL - integration tests don't actually upload avatars
        return Task.FromResult($"https://test.blob.core.windows.net/avatars/{Guid.NewGuid()}.jpg");
    }

    public Task DeleteAvatarAsync(string avatarUrl, CancellationToken ct = default)
    {
        // No-op - integration tests don't actually delete avatars from blob storage
        return Task.CompletedTask;
    }
}
