using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added as entities are created
    // public DbSet<Cart> Carts => Set<Cart>();
    // public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Cart tables in 'cart' schema
        modelBuilder.HasDefaultSchema("cart");

        // Apply configurations from Cart module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CartDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Cart") == true);

        // MassTransit outbox entities will be added in Plan 04
    }
}
