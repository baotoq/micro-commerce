using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// The signed-in Merchant's part of the Catalog, for <see cref="MerchantCentre"/> endpoints. Its
/// <see cref="Products"/> are already scoped to the Merchant (ADR-0001), so another Merchant's Product is simply
/// not found: Merchant Centre endpoints read Products through here, never through <see cref="CatalogDbContext"/>.
/// </summary>
public class MerchantCatalog(CatalogDbContext db, CurrentMerchant currentMerchant, StorefrontProductCache storefrontCache)
{
    public MerchantId MerchantId => currentMerchant.Id;

    public IQueryable<Product> Products
    {
        get
        {
            var merchantId = currentMerchant.Id;
            return db.Products.Where(p => p.MerchantId == merchantId);
        }
    }

    /// <summary>Moves one of the Merchant's Products to another status, unless a Platform Operator removed it.</summary>
    /// <returns>
    /// The changed and saved Product, or null if the Merchant has no Product with the ID; and
    /// <see cref="Product.RemovedMessage"/> instead if it is Removed, in which case nothing changed.
    /// </returns>
    public async Task<(Product? Product, string? Refusal)> ChangeStatusAsync(
        ProductId id, Action<Product> change, CancellationToken ct)
    {
        var product = await Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return (null, null);
        if (product.IsRemoved) return (product, Product.RemovedMessage);

        change(product);

        // Only the status, and only while the Product isn't Removed, so a removal that commits after the read
        // above still wins.
        var status = product.Status;
        var saved = await Products
            .Where(p => p.Id == id && p.Status != ProductStatus.Removed)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, status), ct);
        if (saved == 0) return (product, Product.RemovedMessage);

        await storefrontCache.EvictAsync(product.Id, ct);
        return (product, null);
    }
}
