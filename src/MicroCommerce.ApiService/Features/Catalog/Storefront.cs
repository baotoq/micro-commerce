using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>Whether any Variant has stock left after its active Reservations (<see cref="Availability"/>).</summary>
    public bool InStock { get; init; }
}

internal static class StorefrontQueries
{
    /// <summary>
    /// The Products Buyers can see: Published, and with their Shop in the Catalog. Storefront endpoints read Products
    /// through here, so a Product hidden from Buyers is not found.
    /// </summary>
    public static IQueryable<ShopProduct> VisibleProducts(this CatalogDbContext db)
    {
        var activeReservations = db.ActiveReservations();
        return
            from product in db.Products
            where product.Status == ProductStatus.Published
            join shop in db.Shops on product.MerchantId equals shop.Id
            where !shop.IsSuspended
            select new ShopProduct
            {
                Product = product,
                Shop = shop,
                InStock = product.Variants.Any(v =>
                    v.Stock > activeReservations.Where(r => r.VariantId == v.Id).Sum(r => r.Quantity)),
            };
    }

    /// <summary>The Reservations that count against availability now (<see cref="Reservation.ActiveAt"/>).</summary>
    public static IQueryable<Reservation> ActiveReservations(this CatalogDbContext db) =>
        db.Reservations.Where(Reservation.ActiveAt(DatabaseClock.UtcNow));

    /// <summary>The active reserved quantity of each of <paramref name="productId"/>'s reserved Variants.</summary>
    public static Task<Dictionary<VariantId, int>> ReservedQuantitiesAsync(
        this CatalogDbContext db, ProductId productId, CancellationToken ct) =>
        db.ActiveReservations()
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.VariantId)
            .Select(g => new { VariantId = g.Key, Quantity = g.Sum(r => r.Quantity) })
            .ToDictionaryAsync(x => x.VariantId, x => x.Quantity, ct);
}

/// <summary>One page of a list, and how many items the whole list holds.</summary>
public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
