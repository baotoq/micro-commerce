using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure.Configurations;

public sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => new AdjustmentId(value));

        builder.Property(a => a.StockItemId)
            .HasConversion(
                id => id.Value,
                value => new StockItemId(value));

        builder.Property(a => a.Adjustment)
            .IsRequired();

        builder.Property(a => a.QuantityAfter)
            .IsRequired();

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.AdjustedBy)
            .HasMaxLength(200);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Index on StockItemId for history queries
        builder.HasIndex(a => a.StockItemId);

        // Index on CreatedAt descending for chronological history
        builder.HasIndex(a => a.CreatedAt)
            .IsDescending();
    }
}
