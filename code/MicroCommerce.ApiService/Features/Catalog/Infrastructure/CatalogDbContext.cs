using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added as entities are created
    // public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Catalog tables in 'catalog' schema
        modelBuilder.HasDefaultSchema("catalog");

        // Apply configurations from Catalog module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Catalog") == true);

        // MassTransit outbox entities will be added in Plan 04
    }
}
