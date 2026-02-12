using MicroCommerce.ApiService.Features.Wishlists.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Infrastructure;

public class WishlistsDbContext : DbContext
{
    public WishlistsDbContext(DbContextOptions<WishlistsDbContext> options)
        : base(options)
    {
    }

    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Wishlists tables in 'wishlists' schema
        modelBuilder.HasDefaultSchema("wishlists");

        // Apply configurations from Wishlists module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WishlistsDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Wishlists.Infrastructure") == true);
    }
}
