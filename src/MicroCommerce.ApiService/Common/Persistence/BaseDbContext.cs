using MassTransit;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Converters;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Common.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MassTransit outbox entities — shared "outbox" schema across all feature DbContexts
        // so every context writes to the same outbox tables in the same transaction
        modelBuilder.AddInboxStateEntity(m => m.ToTable("inbox_state", "outbox"));
        modelBuilder.AddOutboxMessageEntity(m => m.ToTable("outbox_message", "outbox"));
        modelBuilder.AddOutboxStateEntity(m => m.ToTable("outbox_state", "outbox"));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.RegisterAllInVogenEfCoreConverters();

        // SmartEnum string converters — stores by Name (compatible with existing string DB schema)
        configurationBuilder.Properties<OrderStatus>()
            .HaveConversion<SmartEnumStringConverter<OrderStatus>>();
        configurationBuilder.Properties<ProductStatus>()
            .HaveConversion<SmartEnumStringConverter<ProductStatus>>();
        configurationBuilder.Properties<DiscountType>()
            .HaveConversion<SmartEnumStringConverter<DiscountType>>();

        configurationBuilder.Conventions.Add(_ => new Conventions.AuditableConvention());
        configurationBuilder.Conventions.Add(_ => new Conventions.ConcurrencyTokenConvention());
        configurationBuilder.Conventions.Add(_ => new Conventions.SoftDeletableConvention());
    }
}
