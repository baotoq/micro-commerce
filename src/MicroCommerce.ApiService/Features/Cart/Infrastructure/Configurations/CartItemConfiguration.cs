using MicroCommerce.ApiService.Features.Cart.Domain.Entities;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new CartItemId(value));

        builder.Property(i => i.CartId)
            .HasConversion(
                id => id.Value,
                value => new CartId(value));

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.ImageUrl);

        builder.Property(i => i.Quantity)
            .IsRequired();
    }
}
