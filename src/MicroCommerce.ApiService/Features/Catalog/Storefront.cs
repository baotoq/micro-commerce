using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>The <c>/storefront/*</c> route group: what anyone can browse on the Marketplace, signed in or not.</summary>
public sealed class Storefront : Group
{
    public Storefront()
    {
        Configure("storefront", ep => ep.AllowAnonymous());
    }
}

/// <summary>A Product a Buyer can see, with the Shop that sells it.</summary>
public class ShopProduct
{
    public required Product Product { get; init; }
    public required Shop Shop { get; init; }
}

internal static class StorefrontQueries
{
    /// <summary>
    /// The Products Buyers can see: Published, and with their Shop in the Catalog. Storefront endpoints read Products
    /// through here, so a Product hidden from Buyers is not found.
    /// </summary>
    public static IQueryable<ShopProduct> VisibleProducts(this CatalogDbContext db) =>
        from product in db.Products
        where product.Status == ProductStatus.Published
        join shop in db.Shops on product.MerchantId equals shop.Id
        select new ShopProduct { Product = product, Shop = shop };
}

/// <summary>One page of a list, and how many items the whole list holds.</summary>
public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
