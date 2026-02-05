using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure;

/// <summary>
/// Seeds StockItem records for existing catalog products.
/// Only runs in Development environment and is idempotent.
/// </summary>
public sealed class InventoryDataSeeder : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<InventoryDataSeeder> _logger;

    public InventoryDataSeeder(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<InventoryDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogInformation("Skipping inventory data seeding - not in Development environment");
            return;
        }

        // Delay to let migrations and catalog seeder run first
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            // Idempotency check: skip if already seeded
            if (await inventoryContext.StockItems.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("Inventory data already exists, skipping seeding");
                return;
            }

            var productIds = await catalogContext.Products
                .Select(p => p.Id.Value)
                .ToListAsync(stoppingToken);

            if (productIds.Count == 0)
            {
                _logger.LogWarning("No products found in catalog, skipping inventory seeding");
                return;
            }

            _logger.LogInformation("Seeding stock for {Count} products...", productIds.Count);

            // Seeded random for reproducibility
            var random = new Random(42);
            var stockItems = new List<StockItem>();
            var adjustments = new List<StockAdjustment>();

            foreach (var productId in productIds)
            {
                var stockItem = StockItem.Create(productId);
                var quantity = GenerateStockQuantity(random);

                if (quantity > 0)
                {
                    stockItem.AdjustStock(quantity, "Initial seed stock", "system");
                }

                stockItems.Add(stockItem);

                var adjustment = StockAdjustment.Create(
                    stockItem.Id,
                    quantity,
                    quantity,
                    "Initial seed stock",
                    "system");
                adjustments.Add(adjustment);
            }

            inventoryContext.StockItems.AddRange(stockItems);
            inventoryContext.StockAdjustments.AddRange(adjustments);
            await inventoryContext.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Seeded stock for {Count} products", productIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding inventory data");
        }
    }

    /// <summary>
    /// Generates a stock quantity with distribution:
    /// ~10% get 0 (out of stock), ~20% get 1-10 (low stock), ~70% get 20-100 (normal stock).
    /// </summary>
    private static int GenerateStockQuantity(Random random)
    {
        var roll = random.Next(100);

        return roll switch
        {
            < 10 => 0,                       // ~10% out of stock
            < 30 => random.Next(1, 11),       // ~20% low stock (1-10)
            _ => random.Next(20, 101)          // ~70% normal stock (20-100)
        };
    }
}
