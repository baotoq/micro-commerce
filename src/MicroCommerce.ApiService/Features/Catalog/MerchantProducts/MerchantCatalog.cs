using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

/// <summary>
/// The signed-in Merchant's own Products. Every Merchant Centre read and write goes through
/// <see cref="Products"/>, never <c>db.Products</c>, so another Merchant's Product is simply not
/// found (FR-016).
/// </summary>
public sealed class MerchantCatalog(CatalogDbContext db, ICurrentMerchant currentMerchant)
{
    public CatalogDbContext Db => db;

    public IQueryable<Product> Products => db.Products.Where(p => p.MerchantId == currentMerchant.Id);
}
