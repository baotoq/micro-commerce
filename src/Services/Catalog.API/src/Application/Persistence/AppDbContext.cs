using MicroCommerce.Catalog.Domain.Analytics;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Marketing;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Domain.Payouts;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroCommerce.Catalog.Application.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Bridge so Infrastructure (which owns IEntityTypeConfiguration implementations)
    // can register them without Application referencing Infrastructure.
    // Infrastructure sets this in its DI bootstrap before the context is first used.
    public static Action<ModelBuilder>? ConfigureModel { get; set; }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Promotion> Promotions => Set<Promotion>();

    // Seller domains (Orders is the foundation; the rest derive from it).
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<AnalyticsSource> AnalyticsSources => Set<AnalyticsSource>();
    public DbSet<AnalyticsFunnelStage> AnalyticsFunnelStages => Set<AnalyticsFunnelStage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel?.Invoke(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Npgsql maps DateTimeOffset to `timestamptz`, which only accepts UTC (Offset 0).
        // Callers (seeders, command handlers, the demo clock) use Pacific-offset literals,
        // so normalize every DateTimeOffset to UTC on write. The instant is preserved; the
        // frontend re-projects to America/Los_Angeles for display. Applies to DateTimeOffset?
        // too. No column-type change, so no migration impact.
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<UtcDateTimeOffsetConverter>();
    }
}

/// <summary>Stores DateTimeOffset as UTC (Offset 0) for Npgsql timestamptz; identity on read.</summary>
public sealed class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public UtcDateTimeOffsetConverter() : base(v => v.ToUniversalTime(), v => v) { }
}
