using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Category entity.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        // Name value object conversion
        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => CategoryName.Create(value))
            .HasMaxLength(CategoryName.MaxLength)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Index for name uniqueness
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("ix_categories_name");
    }
}
