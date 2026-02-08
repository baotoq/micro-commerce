using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added as entities are created
    // public DbSet<Order> Orders => Set<Order>();
    // public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Ordering tables in 'ordering' schema
        modelBuilder.HasDefaultSchema("ordering");

        // Apply configurations from Ordering module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderingDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Ordering") == true);

        // MassTransit outbox entities will be added in Plan 04
    }
}
