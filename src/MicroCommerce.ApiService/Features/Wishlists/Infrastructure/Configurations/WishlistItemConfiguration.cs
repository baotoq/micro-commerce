using MicroCommerce.ApiService.Features.Wishlists.Domain.Entities;
using MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Wishlists.Infrastructure.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(w => w.Id);

        // Strongly-typed ID conversion
        builder.Property(w => w.Id)
            .HasConversion(
                id => id.Value,
                value => new WishlistItemId(value))
            .ValueGeneratedNever();

        // Composite unique index - one wishlist entry per user per product
        builder.HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique();

        // Index on UserId for listing user's wishlist
        builder.HasIndex(w => w.UserId);

        // Index on AddedAt descending for chronological sort
        builder.HasIndex(w => w.AddedAt)
            .IsDescending();

        builder.Property(w => w.UserId)
            .IsRequired();

        builder.Property(w => w.ProductId)
            .IsRequired();

        builder.Property(w => w.AddedAt)
            .IsRequired();

        // PostgreSQL xmin for optimistic concurrency
        builder.Property(w => w.Version)
            .IsRowVersion();
    }
}
