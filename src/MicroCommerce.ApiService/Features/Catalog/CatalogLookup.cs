using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// The Catalog's in-process contract for other modules (ADR-0006): IDs and plain values in and out, never Catalog
/// entities. Ordering reads Variants only through here.
/// </summary>
public interface ICatalogLookup
{
    /// <summary>
    /// The Variants with the given IDs, keyed by ID, in any status. A Variant that no longer exists is simply absent.
    /// </summary>
    Task<IReadOnlyDictionary<VariantId, VariantLookup>> LookupAsync(IReadOnlyCollection<VariantId> variantIds, CancellationToken ct);
}

/// <summary>
/// A Variant as another module sees it: enough to show it in a Cart, plus its price, weight, owner and availability.
/// </summary>
/// <param name="ShopName">Null until the Merchant's Shop reaches the Catalog.</param>
/// <param name="Available">What can be bought now: the Variant's stock less its active Reservations (<see cref="Availability"/>).</param>
/// <param name="IsBuyable">
/// Whether a Buyer can see it: its Product is Published and its Shop is in the Catalog and not suspended
/// (<see cref="StorefrontQueries.VisibleProducts"/>). Says nothing about stock.
/// </param>
public record VariantLookup(
    VariantId VariantId,
    ProductId ProductId,
    string ProductName,
    IReadOnlyList<string> OptionValues,
    MerchantId MerchantId,
    string? ShopName,
    Money UnitPrice,
    int WeightGrams,
    int Available,
    bool IsBuyable);

public class CatalogLookup(CatalogDbContext db) : ICatalogLookup
{
    public async Task<IReadOnlyDictionary<VariantId, VariantLookup>> LookupAsync(
        IReadOnlyCollection<VariantId> variantIds, CancellationToken ct)
    {
        if (variantIds.Count == 0) return new Dictionary<VariantId, VariantLookup>();

        var ids = variantIds.Distinct().ToList();
        var visibleProductIds = db.VisibleProducts().Select(x => x.Product.Id);
        var activeReservations = db.ActiveReservations();
        var variants = await (
            from product in db.Products
            from variant in product.Variants
            where ids.Contains(variant.Id)
            join shop in db.Shops on product.MerchantId equals shop.Id into shops
            from shop in shops.DefaultIfEmpty()
            select new
            {
                Lookup = new VariantLookup(
                    variant.Id,
                    product.Id,
                    product.Name,
                    variant.OptionValues,
                    product.MerchantId,
                    shop == null ? null : shop.ShopName,
                    variant.Price,
                    variant.WeightGrams,
                    variant.Stock,
                    visibleProductIds.Contains(product.Id)),
                Reserved = activeReservations.Where(r => r.VariantId == variant.Id).Sum(r => r.Quantity),
            })
            .AsNoTracking()
            .ToListAsync(ct);

        return variants.ToDictionary(
            v => v.Lookup.VariantId,
            v => v.Lookup with { Available = Availability.Of(v.Lookup.Available, v.Reserved) });
    }
}
