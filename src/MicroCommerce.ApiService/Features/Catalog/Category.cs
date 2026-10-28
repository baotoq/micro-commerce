using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
/// holding Products can't gain children, and a Category with children or Products can't be deleted. Check
/// them inside <see cref="SaveWithCategoryLockedAsync"/>, so they still hold when the change commits.
/// </summary>
internal static class CategoryRules
{
    public const string UnknownCategory = "No Category has this ID.";
    public const string HasChildren = "This Category has child Categories.";
    public const string HasProducts = "This Category holds Products.";
    public const string NotALeaf = "Products can only be listed in a Category without child Categories.";

    /// <returns>Why the Category can't gain a child, or null if it can.</returns>
    public static async Task<string?> WhyCannotGainChildAsync(this CatalogDbContext db, CategoryId id, CancellationToken ct) =>
        await db.Products.AnyAsync(p => p.CategoryId == id, ct) ? HasProducts : null;

    /// <returns>Why the Category can't hold Products, or null if it can.</returns>
    public static async Task<string?> WhyCannotHoldProductsAsync(this CatalogDbContext db, CategoryId id, CancellationToken ct) =>
        await db.Categories.AnyAsync(c => c.ParentId == id, ct) ? NotALeaf : null;

    /// <returns>Why the Category can't be deleted, or null if it can.</returns>
    public static async Task<string?> WhyCannotBeDeletedAsync(this CatalogDbContext db, CategoryId id, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(c => c.ParentId == id, ct)) return HasChildren;
        if (await db.Products.AnyAsync(p => p.CategoryId == id, ct)) return HasProducts;
        return null;
    }

    /// <summary>
    /// Saves the context's changes if <paramref name="categoryId"/> is a leaf Category, keeping it one until they
    /// commit.
    /// </summary>
    /// <returns>Why the Category can't hold Products, or null once the changes are saved.</returns>
    public static async Task<string?> SaveInLeafCategoryAsync(this CatalogDbContext db, CategoryId categoryId, CancellationToken ct)
    {
        var (found, reason) = await db.SaveWithCategoryLockedAsync(
            categoryId, () => db.WhyCannotHoldProductsAsync(categoryId, ct), ct);
        return found ? reason : UnknownCategory;
    }

    /// <summary>
    /// Saves the context's changes in a transaction holding the Category's row lock, if <paramref name="check"/>
    /// finds no reason not to. Adding a child, adding a Product and deleting all lock the Category they depend
    /// on, so they take turns: a Product can't land in a Category that is concurrently gaining a child, nor a
    /// child under one concurrently gaining a Product, nor either under one being deleted.
    /// </summary>
    /// <param name="check">
    /// Returns why the change is refused, or null to save it; it may also stage more changes. It runs again if
    /// the database connection fails transiently, so it must only read and stage.
    /// </param>
    /// <returns>
    /// Whether the Category exists (nothing is checked or saved if not), and <paramref name="check"/>'s reason
    /// for refusing, which is null once the changes are saved.
    /// </returns>
    public static async Task<(bool Found, string? Reason)> SaveWithCategoryLockedAsync(
        this CatalogDbContext db, CategoryId id, Func<Task<string?>> check, CancellationToken ct)
    {
        var result = await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db.Database
                .SqlQuery<Guid>($"""SELECT "Id" AS "Value" FROM catalog."Categories" WHERE "Id" = {id.Value} FOR UPDATE""")
                .ToListAsync(ct);
            if (locked.Count == 0) return (false, null);

            if (await check() is { } reason) return (true, reason);

            // Keep the changes pending until the commit succeeds, so a retry after a failed commit saves them again.
            await db.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct);
            await transaction.CommitAsync(ct);
            return (true, (string?)null);
        });

        if (result is (true, null)) db.ChangeTracker.AcceptAllChanges();
        return result;
    }
}
