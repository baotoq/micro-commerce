using MicroCommerce.ApiService.Features.Reviews.Domain.Entities;
using MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Reviews.Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        // Strongly-typed ID conversion
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new ReviewId(value))
            .ValueGeneratedNever();

        // Composite unique index - one review per user per product
        builder.HasIndex(r => new { r.UserId, r.ProductId })
            .IsUnique();

        // Index on ProductId for listing reviews by product
        builder.HasIndex(r => r.ProductId);

        // Index on CreatedAt descending for chronological sort
        builder.HasIndex(r => r.CreatedAt)
            .IsDescending();

        builder.Property(r => r.ProductId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        // Rating owned value object
        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(rt => rt.Value)
                .HasColumnName("Rating")
                .IsRequired();
        });

        // ReviewText owned value object
        builder.OwnsOne(r => r.Text, text =>
        {
            text.Property(t => t.Value)
                .HasColumnName("ReviewText")
                .HasMaxLength(1000)
                .IsRequired();
        });

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // PostgreSQL xmin for optimistic concurrency
        builder.Property(r => r.Version)
            .IsRowVersion();

        // Ignore domain events (handled by interceptor)
        builder.Ignore(r => r.DomainEvents);
    }
}
