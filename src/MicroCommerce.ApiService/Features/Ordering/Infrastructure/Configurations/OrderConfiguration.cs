using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value));

        builder.Property(o => o.OrderNumber)
            .HasConversion(
                on => on.Value,
                value => OrderNumber.From(value))
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.BuyerId)
            .IsRequired();

        builder.HasIndex(o => o.BuyerId);

        builder.Property(o => o.BuyerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(a => a.Name)
                .HasColumnName("ShippingAddress_Name")
                .HasMaxLength(200)
                .IsRequired();

            sa.Property(a => a.Email)
                .HasColumnName("ShippingAddress_Email")
                .HasMaxLength(256)
                .IsRequired();

            sa.Property(a => a.Street)
                .HasColumnName("ShippingAddress_Street")
                .HasMaxLength(500)
                .IsRequired();

            sa.Property(a => a.City)
                .HasColumnName("ShippingAddress_City")
                .HasMaxLength(100)
                .IsRequired();

            sa.Property(a => a.State)
                .HasColumnName("ShippingAddress_State")
                .HasMaxLength(100)
                .IsRequired();

            sa.Property(a => a.ZipCode)
                .HasColumnName("ShippingAddress_ZipCode")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(o => o.Tax)
            .HasPrecision(18, 2);

        builder.Property(o => o.Total)
            .HasPrecision(18, 2);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.FailureReason)
            .HasMaxLength(1000);

        // xmin optimistic concurrency token for PostgreSQL
        builder.Property(o => o.Version)
            .IsRowVersion();

        // Cart owns OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Index on CreatedAt descending for recent orders
        builder.HasIndex(o => o.CreatedAt)
            .IsDescending();
    }
}
