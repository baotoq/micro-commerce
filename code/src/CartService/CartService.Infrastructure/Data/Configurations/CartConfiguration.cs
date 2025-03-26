using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.CartService.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CartId(value))
            .ValueGeneratedNever();

        builder.OwnsMany(c => c.Items, itemsBuilder =>
        {
            itemsBuilder.WithOwner();

            itemsBuilder.Property(s => s.CartItemId)
                .HasConversion(id => id.Value, value => new CartItemId(value))
                .IsRequired();

            itemsBuilder.Property(s => s.Quantity)
                .IsRequired();

            itemsBuilder.Property(s => s.PriceAtPurchase)
                .HasConversion(s => s.Amount, value => new Money(value))
                .IsRequired();
        });
    }
}
