using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

public sealed record CategoryNode(Guid Id, string Name, IReadOnlyList<CategoryNode> Children);

/// <summary>The whole Category tree, for anyone (FR-033). Siblings are sorted by name, ignoring case.</summary>
public sealed class GetCategoryTreeEndpoint(CatalogDbContext db) : EndpointWithoutRequest<IReadOnlyList<CategoryNode>>
{
    public override void Configure()
    {
        Get("/categories");
        Group<MarketplaceGroup>();
        Summary(s =>
        {
            s.Summary = "The Category tree";
            s.Responses[200] = "Every Category, nested under its parent.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Platform Operators keep the tree small, so it is loaded whole (research R4).
        var categories = await db.Categories.AsNoTracking().ToListAsync(ct);
        var byParent = categories.ToLookup(c => c.ParentId?.Value);

        IReadOnlyList<CategoryNode> Build(Guid? parentId) => byParent[parentId]
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .Select(c => new CategoryNode(c.Id.Value, c.Name, Build(c.Id.Value)))
            .ToList();

        await Send.OkAsync(Build(null), ct);
    }
}
