using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Categories;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

/// <summary>Shared steps of the Merchant Centre Product endpoints.</summary>
internal static class ProductEndpointSupport
{
    public const string NotALeaf = "categoryId must be an existing Category without child Categories.";

    /// <summary>
    /// Saves after checking, under the target Category's row lock, that it exists and is a leaf
    /// (FR-007, FR-035). Returns false, saving nothing, when it isn't.
    /// </summary>
    public static async Task<bool> SaveInLeafAsync(CatalogDbContext db, CategoryId categoryId, Action stage, CancellationToken ct)
    {
        var isLeaf = false;
        await CategoryRules.SaveWithCategoryLockedAsync(db, categoryId, async (category, token) =>
        {
            isLeaf = category is not null && !await CategoryRules.HasChildrenAsync(db, categoryId, token);
            if (isLeaf) stage();
        }, ct);
        return isLeaf;
    }

    /// <summary>
    /// Loads the signed-in Merchant's Product for a change, or sends 404 or 409 and returns null:
    /// 404 when it isn't theirs, 409 product-removed when a Platform Operator removed it, and 409
    /// product-version-conflict when <paramref name="version"/> is stale.
    /// </summary>
    public static async Task<Product?> LoadForChangeAsync<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> endpoint, MerchantCatalog catalog, Guid id, int? version, CancellationToken ct)
        where TRequest : notnull
    {
        var productId = ProductId.From(id);
        var product = await catalog.Products.SingleOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
        {
            await endpoint.HttpContext.Response.SendNotFoundAsync(ct);
            return null;
        }

        if (product.Status == ProductStatus.Removed)
        {
            await Problems.Conflict(Problems.ProductRemoved).ExecuteAsync(endpoint.HttpContext);
            return null;
        }

        if (version is not null && product.Version != version)
        {
            await Problems.Conflict(Problems.ProductVersionConflict).ExecuteAsync(endpoint.HttpContext);
            return null;
        }

        return product;
    }

    /// <summary>Saves a change, or sends 409 product-version-conflict when a concurrent save won. Returns whether it saved.</summary>
    public static async Task<bool> TrySaveAsync(HttpContext http, CatalogDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await Problems.Conflict(Problems.ProductVersionConflict).ExecuteAsync(http);
            return false;
        }
    }
}
