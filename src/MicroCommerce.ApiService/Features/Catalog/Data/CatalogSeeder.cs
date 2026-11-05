using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>
/// Development only: seeds a starter Category tree when there are no Categories yet, so the
/// Marketplace can be demonstrated without setup (FR-033). <c>DatabaseMigrator</c> runs it before it
/// reports the database ready, so nothing races it (research R2, R4).
/// </summary>
public static class CatalogSeeder
{
    private static readonly (string Name, (string Name, string[] Children)[] Children)[] StarterTree =
    [
        ("Fashion", [("Men", ["T-Shirts", "Shirts"]), ("Women", ["Dresses", "Tops"])]),
        ("Home", [("Kitchen", []), ("Bedding", [])]),
        ("Electronics", [("Phones", []), ("Laptops", [])]),
    ];

    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(ct)) return;

        foreach (var (name, children) in StarterTree)
        {
            var top = Category.CreateTopLevel(CategoryId.New(), name);
            db.Categories.Add(top);
            foreach (var (childName, grandchildren) in children)
            {
                var child = Category.CreateChild(CategoryId.New(), childName, top.Id);
                db.Categories.Add(child);
                foreach (var grandchild in grandchildren)
                {
                    db.Categories.Add(Category.CreateChild(CategoryId.New(), grandchild, child.Id));
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
