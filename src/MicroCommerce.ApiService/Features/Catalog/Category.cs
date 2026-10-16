using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// A node of the Marketplace-wide Category tree, maintained by Platform Operators. A Category without a parent
/// is top-level. Every Product sits in exactly one leaf Category.
/// </summary>
public class Category
{
    public const int NameMaxLength = 100;

    public CategoryId Id { get; init; } = CategoryId.New();
    public required string Name { get; set; }

    /// <summary>The parent Category; null for a top-level Category. A Category doesn't move once created.</summary>
    public CategoryId? ParentId { get; init; }

    public List<Category> Children { get; } = [];
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(Category.NameMaxLength).IsRequired();

        // The database refuses to orphan children too, should a child be added while its parent is deleted.
        builder.HasMany(c => c.Children).WithOne().HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>A single Category, as an operator creates or renames it.</summary>
public record CategoryResponse(CategoryId Id, string Name, CategoryId? ParentId);

/// <summary>A Category and its descendants, children ordered by name.</summary>
public record CategoryNodeResponse(CategoryId Id, string Name, IReadOnlyList<CategoryNodeResponse> Children);

/// <summary>
/// The tree rules that depend on what a Category holds. Every Product sits in a leaf Category, so a Category
/// holding Products can't gain children, and a Category with children or Products can't be deleted.
/// </summary>
internal static class CategoryRules
{
    public const string UnknownParent = "No Category has this ID.";
    public const string HasChildren = "This Category has child Categories.";

    /// <returns>Why the Category can't gain a child, or null if it can.</returns>
    public static Task<string?> WhyCannotGainChildAsync(this CatalogDbContext db, CategoryId id, CancellationToken ct) =>
        // Catalog has no Products yet. Once it does, a Category holding any can't gain children: check it here.
        Task.FromResult<string?>(null);

    /// <returns>Why the Category can't be deleted, or null if it can.</returns>
    public static async Task<string?> WhyCannotBeDeletedAsync(this CatalogDbContext db, CategoryId id, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(c => c.ParentId == id, ct)) return HasChildren;

        // Catalog has no Products yet. Once it does, a Category holding any can't be deleted either: check it here.
        return null;
    }

    /// <summary>Whether saving failed on the parent foreign key: a parent and child changed concurrently.</summary>
    public static bool IsParentViolation(DbUpdateException e) =>
        e.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation };
}
