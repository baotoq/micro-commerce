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
public class MerchantCatalog(CatalogDbContext db, CurrentMerchant currentMerchant)
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

    /// <returns>The changed and saved Product, or null if the Merchant has no Product with the ID.</returns>
    public async Task<Product?> ChangeStatusAsync(ProductId id, Action<Product> change, CancellationToken ct)
    {
        var product = await Products.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return null;

        change(product);
        await db.SaveChangesAsync(ct);
        return product;
    }
}
