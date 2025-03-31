using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Infrastructure.Data.Extensions;
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

        builder.Property(c => c.Discount).HasMoneyConversion();
        builder.Property(c => c.Total).HasMoneyConversion();

        builder.OwnsMany(c => c.Items, itemsBuilder =>
        {
            itemsBuilder.WithOwner();

            itemsBuilder.Property(s => s.CartItemId)
                .HasConversion(id => id.Value, value => new CartItemId(value))
                .IsRequired();

            itemsBuilder.HasIndex(s => s.CartItemId);

            itemsBuilder.Property(s => s.Quantity)
                .IsRequired();

            itemsBuilder.Property(s => s.UnitPriceAtPurchase)
                .HasMoneyConversion()
                .IsRequired();

            itemsBuilder.Property(s => s.SubTotal)
                .HasMoneyConversion();
        });
    }
}
