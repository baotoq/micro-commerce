using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>
/// The Catalog module's own context: its tables and migration history live in the
/// <see cref="Schema"/> schema, and no other module reads them (ADR-0003).
/// </summary>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Category> Categories => Set<Category>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<CategoryId>().HaveConversion<CategoryId.EfCoreValueConverter, CategoryId.EfCoreValueComparer>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly, IsCatalogType);
    }

    /// <summary>Keeps this context's migration history apart from the other modules'.</summary>
    public static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema);

    private static bool IsCatalogType(Type type) =>
        type.Namespace?.StartsWith(typeof(Category).Namespace!, StringComparison.Ordinal) == true;
}
