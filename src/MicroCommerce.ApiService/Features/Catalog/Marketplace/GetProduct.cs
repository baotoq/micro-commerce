using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

/// <summary>A Published Product's page: images, details and every Variant's price and Availability (FR-029, FR-030).</summary>
public sealed class GetMarketplaceProductEndpoint(CatalogDbContext db, IMerchantDirectory merchants)
    : Endpoint<ProductIdRequest, ProductDetail>
{
    public override void Configure()
    {
        Get("/products/{id}");
        Group<MarketplaceGroup>();
        Summary(s =>
        {
            s.Summary = "A Published Product";
            s.Responses[404] = "No such Product, or it isn't Published.";
        });
    }

    public override async Task HandleAsync(ProductIdRequest req, CancellationToken ct)
    {
        var id = ProductId.From(req.Id);
        var product = await db.Products.AsNoTracking().VisibleToBuyers().SingleOrDefaultAsync(p => p.Id == id, ct);
        var shops = product is null ? null : await merchants.GetShopsAsync([product.MerchantId], ct);
        if (product is null || !shops!.TryGetValue(product.MerchantId, out var shop))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var category = await db.Categories.AsNoTracking().SingleAsync(c => c.Id == product.CategoryId, ct);
        await Send.OkAsync(new ProductDetail(
            product.Id.Value,
            product.Name,
            product.Description,
            new CategoryRef(category.Id.Value, category.Name),
            product.Options.Select(o => new MarketplaceOption(o.Name, o.Values)).ToList(),
            // Availability equals stock until Reservations exist (FR-030).
            product.Variants.Select(v => new MarketplaceVariant(v.Id.Value, v.OptionValues, v.Price.Value, v.Stock)).ToList(),
            product.Images.Select(i => new MarketplaceImage(i.Id.Value, MarketplaceUrls.Image(product.Id, i.Id))).ToList(),
            new ShopSummary(shop.Id.Value, shop.ShopName)), ct);
    }
}
