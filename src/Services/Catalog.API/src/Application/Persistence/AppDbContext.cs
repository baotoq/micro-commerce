using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Bridge so Infrastructure (which owns IEntityTypeConfiguration implementations)
    // can register them without Application referencing Infrastructure.
    // Infrastructure sets this in its DI bootstrap before the context is first used.
    public static Action<ModelBuilder>? ConfigureModel { get; set; }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel?.Invoke(modelBuilder);
    }
}
