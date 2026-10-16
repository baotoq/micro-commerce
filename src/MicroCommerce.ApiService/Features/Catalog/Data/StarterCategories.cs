using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>A starter Category tree, seeded in Development so the Marketplace can be demoed without setup.</summary>
public static class StarterCategories
{
    private static readonly Dictionary<string, Dictionary<string, string[]>> Tree = new()
    {
        ["Fashion"] = new() { ["Men"] = ["T-Shirts", "Shirts"], ["Women"] = ["Dresses", "Tops"] },
        ["Home"] = new() { ["Kitchen"] = [], ["Bedding"] = [] },
        ["Electronics"] = new() { ["Phones"] = [], ["Laptops"] = [] },
    };

    /// <summary>Adds the starter tree to an empty Category tree; a tree operators have started is left alone.</summary>
    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct)) return;

        foreach (var (topName, children) in Tree)
        {
            var top = new Category { Name = topName };
            db.Categories.Add(top);
            foreach (var (childName, grandchildren) in children)
            {
                var child = new Category { Name = childName, ParentId = top.Id };
                db.Categories.Add(child);
                db.Categories.AddRange(grandchildren.Select(name => new Category { Name = name, ParentId = child.Id }));
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
