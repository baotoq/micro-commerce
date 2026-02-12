using MassTransit;
using MicroCommerce.ApiService.Common.Persistence;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public ApiWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
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
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();

            foreach (var service in hostedServices)
            {
                services.Remove(service);
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
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        await _dbContainer.StartAsync();

        // Apply all migrations to the test database
        using var scope = Services.CreateScope();

        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        await outboxDb.Database.MigrateAsync();

        var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await catalogDb.Database.MigrateAsync();

        var cartDb = scope.ServiceProvider.GetRequiredService<CartDbContext>();
        await cartDb.Database.MigrateAsync();

        var orderingDb = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await orderingDb.Database.MigrateAsync();

        var inventoryDb = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await inventoryDb.Database.MigrateAsync();
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
