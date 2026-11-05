using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>Index names the handlers map a unique violation (23505) back from.</summary>
public static class CategoryIndexes
{
    public const string SiblingName = "ix_categories_parent_id_normalized_name";
}

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(Category.MaxNameLength);
        builder.Property(c => c.NormalizedName).HasColumnName("normalized_name").HasMaxLength(Category.MaxNameLength);
        builder.Property(c => c.ParentId).HasColumnName("parent_id");
        builder.Ignore(c => c.DomainEvents);

        builder.HasOne<Category>().WithMany().HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);

        // NULLS NOT DISTINCT: top-level Categories (no parent) are siblings too (FR-034, research R4).
        builder.HasIndex(c => new { c.ParentId, c.NormalizedName })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName(CategoryIndexes.SiblingName);
    }
}
