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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Replaces production dependencies with test equivalents:
/// - PostgreSQL: Testcontainers instance (replaces Aspire Npgsql integration)
/// - MassTransit: In-memory test harness (replaces Azure Service Bus transport)
/// - Azure Blob Storage: No-op stub (image upload not tested here)
/// - Background services: Disabled (data seeders, cleanup services)
/// - Keycloak JWT: Replaced with FakeAuthenticationHandler
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly SoftDeleteInterceptor _softDeleteInterceptor = new();
    private static readonly ConcurrencyInterceptor _concurrencyInterceptor = new();
    private static readonly AuditInterceptor _auditInterceptor = new();

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

            // MassTransit registers 'masstransit-bus' health checks via IConfigureOptions<HealthCheckServiceOptions>.
            // When AddMassTransitTestHarness is called, it re-registers the same checks, causing:
            //   "Duplicate health checks were registered with the name(s): masstransit-bus"
            // Fix: Add a PostConfigure action that deduplicates health check registrations by name.
            // This runs after all IConfigureOptions<HealthCheckServiceOptions> have applied, removing any dupes.
            services.PostConfigure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>(opts =>
            {
                System.Collections.Generic.HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
                System.Collections.Generic.List<HealthCheckRegistration> duplicates = [];
                foreach (HealthCheckRegistration r in opts.Registrations)
                {
                    if (!seen.Add(r.Name))
                    {
                        duplicates.Add(r);
                    }
                }
                foreach (HealthCheckRegistration duplicate in duplicates)
                {
                    opts.Registrations.Remove(duplicate);
                }
            });

            // Replace all DbContext registrations with Testcontainer connection string.
            // Aspire's AddNpgsqlDbContext uses AddDbContextPool which registers additional pool-related
            // services (IDbContextOptionsConfiguration<T>, IDbContextPool<T>, IScopedDbContextLease<T>)
            // that must all be removed to prevent the Aspire connection string validator from running.
            // We remove ALL pool-related descriptors for each DbContext type, then re-register with
            // AddDbContext (non-pooled) pointing at the Testcontainer connection string.

            ReplaceDbContext<OutboxDbContext>(services, "outbox");
            ReplaceDbContext<CatalogDbContext>(services, "catalog");
            ReplaceDbContext<CartDbContext>(services, "cart");
            ReplaceDbContext<OrderingDbContext>(services, "ordering");
            ReplaceDbContext<InventoryDbContext>(services, "inventory");
            ReplaceDbContext<ProfilesDbContext>(services, "profiles");
            ReplaceDbContext<ReviewsDbContext>(services, "reviews");
            ReplaceDbContext<WishlistsDbContext>(services, "wishlists");

            // Replace MassTransit with in-memory test harness.
            // The primary challenge is BusDepot.ctor(IEnumerable<IBusInstance>) which throws
            // "An item with the same key has already been added. Key: MassTransit.IBus"
            // when both the production Azure Service Bus instance AND the test harness bus instance
            // are registered as IBusInstance in the DI container.
            //
            // services.RemoveAll(IBus/IBusControl/IPublishEndpoint/ISendEndpointProvider) removes 4 services
            // but leaves 3 bus-instance-related registrations that cause duplicates:
            //   1. MassTransit.Transports.IBusInstance
            //   2. MassTransit.DependencyInjection.Bind<IBus, IBusInstance>
            //   3. MassTransit.IBusDepot
            // These 3 must also be removed before AddMassTransitTestHarness adds its own versions.
            services.RemoveAll(typeof(IBus));
            services.RemoveAll(typeof(IBusControl));
            services.RemoveAll(typeof(IPublishEndpoint));
            services.RemoveAll(typeof(ISendEndpointProvider));

            System.Collections.Generic.List<ServiceDescriptor> busInstanceDescriptors = services
                .Where(d => d.ServiceType.FullName?.Equals("MassTransit.Transports.IBusInstance", StringComparison.Ordinal) == true
                    || d.ServiceType.FullName?.StartsWith("MassTransit.DependencyInjection.Bind`2", StringComparison.Ordinal) == true
                    || d.ServiceType.FullName?.Equals("MassTransit.IBusDepot", StringComparison.Ordinal) == true)
                .ToList();
            foreach (ServiceDescriptor descriptor in busInstanceDescriptors)
            {
                services.Remove(descriptor);
            }

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

    /// <summary>
    /// Removes all pool-related service registrations for the given DbContext type
    /// (registered by Aspire's AddNpgsqlDbContext/AddDbContextPool) and re-registers
    /// the context with AddDbContext pointing at the Testcontainer connection string.
    /// </summary>
    private void ReplaceDbContext<TContext>(IServiceCollection services, string schema)
        where TContext : DbContext
    {
        // Remove all service descriptors related to this DbContext type.
        // AddDbContextPool registers: DbContextOptions<T>, IDbContextOptionsConfiguration<T>,
        // IDbContextPool<T> (internal), IScopedDbContextLease<T> (internal), and T itself.
        // We use FullName string matching for internal types to avoid EF1001 errors.
        string contextTypeName = typeof(TContext).FullName ?? typeof(TContext).Name;
        System.Collections.Generic.List<ServiceDescriptor> descriptorsToRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(IDbContextOptionsConfiguration<TContext>) ||
                d.ServiceType == typeof(TContext) ||
                // Internal EF Core pool types — matched by FullName to avoid EF1001 errors
                (d.ServiceType.IsGenericType &&
                    (d.ServiceType.GetGenericTypeDefinition().FullName?.StartsWith("Microsoft.EntityFrameworkCore.Internal.IDbContextPool", StringComparison.Ordinal) == true ||
                     d.ServiceType.GetGenericTypeDefinition().FullName?.StartsWith("Microsoft.EntityFrameworkCore.Internal.IScopedDbContextLease", StringComparison.Ordinal) == true) &&
                    d.ServiceType.GenericTypeArguments.Length == 1 &&
                    d.ServiceType.GenericTypeArguments[0].FullName == contextTypeName))
            .ToList();

        foreach (ServiceDescriptor descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(_softDeleteInterceptor, _concurrencyInterceptor, _auditInterceptor);
        });
    }

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        await _dbContainer.StartAsync();

        // Apply migrations for all DbContexts.
        // MigrateAsync is used instead of EnsureCreated because multiple DbContexts share one PostgreSQL
        // database (each uses a separate schema). EnsureCreated skips if ANY tables exist in the DB,
        // so only the first context's schema would be created. MigrateAsync applies each context's
        // migrations independently via separate __EFMigrationsHistory tables (one per schema).
        using Microsoft.Extensions.DependencyInjection.IServiceScope scope = Services.CreateScope();

        OutboxDbContext outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        await outboxDb.Database.MigrateAsync();

        CatalogDbContext catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await catalogDb.Database.MigrateAsync();

        CartDbContext cartDb = scope.ServiceProvider.GetRequiredService<CartDbContext>();
        await cartDb.Database.MigrateAsync();

        OrderingDbContext orderingDb = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await orderingDb.Database.MigrateAsync();

        InventoryDbContext inventoryDb = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await inventoryDb.Database.MigrateAsync();

        ProfilesDbContext profilesDb = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
        await profilesDb.Database.MigrateAsync();

        ReviewsDbContext reviewsDb = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();
        await reviewsDb.Database.MigrateAsync();

        WishlistsDbContext wishlistsDb = scope.ServiceProvider.GetRequiredService<WishlistsDbContext>();
        await wishlistsDb.Database.MigrateAsync();
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
