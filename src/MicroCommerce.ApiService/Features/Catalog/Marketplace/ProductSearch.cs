using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

public sealed record MarketplaceQuery(string? Text, CategoryId? CategoryId, MerchantId? ShopId, int Page, int PageSize);

/// <summary>One Product card's data, before Shop names are added.</summary>
public sealed record ProductCardRow(
    ProductId Id, string Name, CategoryId CategoryId, MerchantId MerchantId, Money MinPrice, Money MaxPrice, bool Available, ProductImageId? CoverImageId);

/// <summary>What Buyers may see (FR-025). The Merchant suspension feature will add its rule here, and only here.</summary>
public static class Visibility
{
    public static IQueryable<Product> VisibleToBuyers(this IQueryable<Product> products) =>
        products.Where(p => p.Status == ProductStatus.Published);
}

/// <summary>Marketplace search. Behind an interface so a dedicated engine can replace Postgres later (research R6).</summary>
public interface IProductSearch
{
    Task<(IReadOnlyList<ProductCardRow> Rows, int TotalCount)> SearchAsync(MarketplaceQuery query, CancellationToken ct);
}

/// <summary>
/// Postgres full-text search over Product name and description, with English stemming
/// (<c>websearch_to_tsquery</c>: every word must match), plus a Category filter that includes
/// descendants (FR-027, FR-028).
/// </summary>
public sealed class PostgresProductSearch(CatalogDbContext db) : IProductSearch
{
    private const string Config = "english";

    public async Task<(IReadOnlyList<ProductCardRow> Rows, int TotalCount)> SearchAsync(MarketplaceQuery q, CancellationToken ct)
    {
        var products = db.Products.AsNoTracking().VisibleToBuyers();

        if (q.ShopId is { } shopId)
            products = products.Where(p => p.MerchantId == shopId);

        if (q.CategoryId is { } categoryId)
        {
            var ids = await WithDescendantsAsync(categoryId, ct);
            products = products.Where(p => ids.Contains(p.CategoryId));
        }

        IOrderedQueryable<Product> ordered;
        if (!string.IsNullOrWhiteSpace(q.Text))
        {
            var text = q.Text;
            products = products.Where(p =>
                EF.Property<NpgsqlTsVector>(p, ProductConfiguration.SearchVector).Matches(EF.Functions.WebSearchToTsQuery(Config, text)));
            ordered = products
                .OrderByDescending(p =>
                    EF.Property<NpgsqlTsVector>(p, ProductConfiguration.SearchVector).Rank(EF.Functions.WebSearchToTsQuery(Config, text)))
                .ThenByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id);
        }
        else
        {
            ordered = products.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);
        }

        var total = await products.CountAsync(ct);
        var rows = await ordered
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => new ProductCardRow(
                p.Id,
                p.Name,
                p.CategoryId,
                p.MerchantId,
                p.Variants.Min(v => v.Price),
                p.Variants.Max(v => v.Price),
                // Availability equals stock until Reservations exist (FR-030).
                p.Variants.Any(v => v.Stock > 0),
                p.Images.Where(i => i.Position == 0).Select(i => (ProductImageId?)i.Id).FirstOrDefault()))
            .ToListAsync(ct);

        return (rows, total);
    }

    /// <summary>The Category and all its descendants. Operators keep the tree small, so it is walked in memory (research R4).</summary>
    private async Task<List<CategoryId>> WithDescendantsAsync(CategoryId root, CancellationToken ct)
    {
        var all = await db.Categories.AsNoTracking().ToListAsync(ct);
        if (all.All(c => c.Id != root)) return [];

        var children = all.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value);
        var result = new List<CategoryId>();
        var queue = new Queue<CategoryId>([root]);
        while (queue.TryDequeue(out var id))
        {
            result.Add(id);
            foreach (var child in children[id]) queue.Enqueue(child.Id);
        }

        return result;
    }
}
