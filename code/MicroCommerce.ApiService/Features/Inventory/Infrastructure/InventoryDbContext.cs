using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added as entities are created
    // public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    // public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Inventory tables in 'inventory' schema
        modelBuilder.HasDefaultSchema("inventory");

        // Apply configurations from Inventory module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Inventory") == true);

        // MassTransit outbox entities will be added in Plan 04
    }
}
