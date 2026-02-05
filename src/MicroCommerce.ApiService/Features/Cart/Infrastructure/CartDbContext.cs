using MicroCommerce.ApiService.Features.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Entities.Cart> Carts => Set<Domain.Entities.Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Cart tables in 'cart' schema
        modelBuilder.HasDefaultSchema("cart");

        // Apply configurations from Cart module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CartDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Cart") == true);
    }
}
