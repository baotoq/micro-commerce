using MicroCommerce.ApiService.SharedKernel;
using MicroCommerce.ApiService.Features.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MicroCommerce.ApiService.Features.Ordering.Data;

/// <summary>
/// The Ordering module's own context: its tables and migration history live in the
/// <see cref="Schema"/> schema, and no other module reads them (ADR-0003). It reads Variants only through
/// <see cref="Catalog.ICatalogLookup"/> (ADR-0006).
/// </summary>
public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public const string Schema = "ordering";

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Checkout> Checkouts => Set<Checkout>();
    public DbSet<CheckoutMerchant> CheckoutMerchants => Set<CheckoutMerchant>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AccountId>().HaveConversion<AccountId.EfCoreValueConverter, AccountId.EfCoreValueComparer>();
        configurationBuilder.Properties<MerchantId>().HaveConversion<MerchantId.EfCoreValueConverter, MerchantId.EfCoreValueComparer>();
        configurationBuilder.Properties<ProductId>().HaveConversion<ProductId.EfCoreValueConverter, ProductId.EfCoreValueComparer>();
        configurationBuilder.Properties<VariantId>().HaveConversion<VariantId.EfCoreValueConverter, VariantId.EfCoreValueComparer>();
        configurationBuilder.Properties<CheckoutId>().HaveConversion<CheckoutId.EfCoreValueConverter, CheckoutId.EfCoreValueComparer>();
        configurationBuilder.Properties<Money>().HaveConversion<Money.EfCoreValueConverter, Money.EfCoreValueComparer>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly, IsOrderingType);
    }

    /// <summary>Keeps this context's migration history apart from the other modules'.</summary>
    public static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema);

    private static bool IsOrderingType(Type type) =>
        type.Namespace?.StartsWith(typeof(Cart).Namespace!, StringComparison.Ordinal) == true;
}
