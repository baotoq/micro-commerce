using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>
/// The Catalog area's data, in its own <c>catalog</c> schema with its own migration history. No other
/// area reads these tables (Principle II, research R2).
/// </summary>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            t => t.Namespace?.StartsWith(typeof(CatalogDbContext).Namespace!, StringComparison.Ordinal) == true);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInVogenEfCoreConverters();

    /// <summary>Keeps the migration history in this area's schema.</summary>
    public static void ConfigureNpgsql(Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schema);
}
