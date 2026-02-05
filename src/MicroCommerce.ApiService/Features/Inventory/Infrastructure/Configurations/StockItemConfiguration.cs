using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new StockItemId(value));

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.HasIndex(s => s.ProductId)
            .IsUnique();

        builder.Property(s => s.QuantityOnHand)
            .IsRequired();

        // xmin optimistic concurrency token for PostgreSQL
        builder.Property(s => s.Version)
            .IsRowVersion();

        // Reservations owned by StockItem
        builder.HasMany(s => s.Reservations)
            .WithOne()
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore computed property
        builder.Ignore(s => s.AvailableQuantity);
    }
}
