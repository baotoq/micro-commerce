using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Category entity.
/// Reference implementation for value object and strongly-typed ID mapping.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        // Primary key with strongly-typed ID conversion
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CategoryId.From(value))
            .HasColumnName("id");

        // Name value object conversion
        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => CategoryName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(CategoryName.MaxLength)
            .IsRequired();

        // Simple properties
        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Index for name uniqueness
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("ix_categories_name");
    }
}
