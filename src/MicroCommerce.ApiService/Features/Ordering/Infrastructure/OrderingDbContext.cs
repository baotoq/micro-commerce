using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CheckoutState> CheckoutSagas => Set<CheckoutState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Ordering tables in 'ordering' schema
        modelBuilder.HasDefaultSchema("ordering");

        // Apply configurations from Ordering module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderingDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Ordering") == true);
    }
}
