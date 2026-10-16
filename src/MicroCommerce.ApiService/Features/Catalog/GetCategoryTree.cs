using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>The whole Category tree: its top-level Categories, each with its descendants, ordered by name.</summary>
public class GetCategoryTreeEndpoint(CatalogDbContext db) : EndpointWithoutRequest<IReadOnlyList<CategoryNodeResponse>>
{
    public override void Configure()
    {
        Get("/categories");
        Group<Storefront>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Loading every Category into one context links each to its children.
        var categories = await db.Categories.ToListAsync(ct);
        foreach (var category in categories)
            category.Children.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));

        await Send.OkAsync(
            categories
                .Where(c => c.ParentId is null)
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .Select(c => c.ToNodeResponse())
                .ToList(),
            ct);
    }
}
