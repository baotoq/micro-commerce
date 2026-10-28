using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>What a Buyer looks for on the storefront; every filter given must match.</summary>
public record ProductSearchQuery
{
    /// <summary>Words to find in the Product's name or description, matching their variations ("mugs" finds "mug").</summary>
    public string? Text { get; init; }

    /// <summary>Only Products in this Category or any of its descendants.</summary>
    public CategoryId? CategoryId { get; init; }

    /// <summary>Only this Shop's Products.</summary>
    public MerchantId? ShopId { get; init; }

    /// <summary>1-based.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 24;
}

/// <summary>
/// Finds the Products Buyers can see. Storefront endpoints search through here rather than querying Products
/// themselves, so a dedicated search engine can replace <see cref="PostgresProductSearch"/> later.
/// </summary>
public interface IProductSearch
{
    /// <returns>
    /// One page of the matching Products: the best matches first when searching for <see cref="ProductSearchQuery.Text"/>,
    /// otherwise (and among equal matches) the newest first.
    /// </returns>
    Task<PagedResponse<StorefrontProductSummaryResponse>> SearchAsync(ProductSearchQuery query, CancellationToken ct);
}

/// <summary>
/// Postgres full-text search over each Product's stored, indexed <see cref="SearchVector"/>, built from its name and
/// description with the <see cref="TextSearchConfig"/> configuration, which stems words.
/// </summary>
public class PostgresProductSearch(CatalogDbContext db) : IProductSearch
{
    /// <summary>The Products' generated <c>tsvector</c> column, a shadow property so it stays out of the domain.</summary>
    public const string SearchVector = nameof(SearchVector);

    public const string TextSearchConfig = "english";

    public async Task<PagedResponse<StorefrontProductSummaryResponse>> SearchAsync(ProductSearchQuery query, CancellationToken ct)
    {
        var products = db.VisibleProducts();
        if (query.ShopId is { } shopId) products = products.Where(x => x.Shop.Id == shopId);
        if (query.CategoryId is { } categoryId)
        {
            var categoryIds = await CategoryAndDescendantsAsync(categoryId, ct);
            products = products.Where(x => categoryIds.Contains(x.Product.CategoryId));
        }

        IOrderedQueryable<ShopProduct> ordered;
        if (string.IsNullOrWhiteSpace(query.Text))
        {
            ordered = products.OrderByDescending(x => x.Product.CreatedAt);
        }
        else
        {
            var text = query.Text.Trim();
            ordered = products
                .Where(x => EF.Property<NpgsqlTsVector>(x.Product, SearchVector)
                    .Matches(EF.Functions.WebSearchToTsQuery(TextSearchConfig, text)))
                .OrderByDescending(x => EF.Property<NpgsqlTsVector>(x.Product, SearchVector)
                    .Rank(EF.Functions.WebSearchToTsQuery(TextSearchConfig, text)))
                .ThenByDescending(x => x.Product.CreatedAt);
        }

        var totalCount = await ordered.CountAsync(ct);
        var items = await ordered
            .ThenByDescending(x => x.Product.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToStorefrontSummaryResponse()
            .ToListAsync(ct);

        return new(items, query.Page, query.PageSize, totalCount);
    }

    /// <summary>The Category and everything below it; just the ID itself if no Category has it.</summary>
    private async Task<List<CategoryId>> CategoryAndDescendantsAsync(CategoryId id, CancellationToken ct)
    {
        // Operators keep the tree small, so walking it in memory beats a recursive query.
        var childrenByParent = (await db.Categories.Where(c => c.ParentId != null).Select(c => new { c.Id, c.ParentId }).ToListAsync(ct))
            .ToLookup(c => c.ParentId!.Value, c => c.Id);

        var found = new List<CategoryId>();
        var pending = new Queue<CategoryId>([id]);
        while (pending.TryDequeue(out var next))
        {
            found.Add(next);
            foreach (var child in childrenByParent[next]) pending.Enqueue(child);
        }

        return found;
    }
}
