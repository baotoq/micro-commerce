using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CartProductMapConfiguration : IEntityTypeConfiguration<CartProductMap>
{
    public void Configure(EntityTypeBuilder<CartProductMap> builder)
    {
        builder
            .HasKey(s => new { s.ProductId, s.CartId });
        
        builder
            .HasOne(s => s.Product)
            .WithMany(s => s.CartProductMaps)
            .HasForeignKey(e => e.ProductId)
            .IsRequired();
        
        builder
            .HasOne(s => s.Cart)
            .WithMany(s => s.CartProductMaps)
            .HasForeignKey(e => e.CartId)
            .IsRequired();
    }
}