using MicroCommerce.ApiService.Features.Cart.Domain.Entities;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Domain.Entities.Cart>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new CartId(value));

        builder.Property(c => c.BuyerId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.LastModifiedAt)
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .IsRequired();

        // xmin optimistic concurrency token for PostgreSQL
        builder.Property(c => c.Version)
            .IsRowVersion();

        // Cart owns CartItems
        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Index on BuyerId for fast lookups
        builder.HasIndex(c => c.BuyerId);

        // Index on ExpiresAt for expiration cleanup queries
        builder.HasIndex(c => c.ExpiresAt);
    }
}
