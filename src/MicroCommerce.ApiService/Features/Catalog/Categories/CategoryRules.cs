using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Categories;

/// <summary>
/// Keeps every Product in a leaf Category when changes race (FR-035, research R4). Adding a child,
/// putting a Product into a Category and deleting a Category all lock that Category's row first, so
/// they run one after another and each sees what the other committed.
/// </summary>
public static class CategoryRules
{
    /// <summary>
    /// Locks <paramref name="categoryId"/>'s row, runs <paramref name="check"/> (which gets the locked
    /// Category, or null if there is none), then saves and commits. <paramref name="check"/> runs again
    /// on a transient retry, so it must only read and stage changes.
    /// </summary>
    public static async Task SaveWithCategoryLockedAsync(
        CatalogDbContext db,
        CategoryId categoryId,
        Func<Category?, CancellationToken, Task> check,
        CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db.Categories
                .FromSql($"SELECT * FROM catalog.categories WHERE id = {categoryId.Value} FOR UPDATE")
                .SingleOrDefaultAsync(ct);

            await check(locked, ct);

            await db.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct);
            await transaction.CommitAsync(ct);
        });

        db.ChangeTracker.AcceptAllChanges();
    }

    /// <summary>Whether any Product sits in the Category.</summary>
    public static Task<bool> HoldsProductsAsync(CatalogDbContext db, CategoryId id, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.CategoryId == id, ct);

    /// <summary>Whether the Category has child Categories.</summary>
    public static Task<bool> HasChildrenAsync(CatalogDbContext db, CategoryId id, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.ParentId == id, ct);
}
