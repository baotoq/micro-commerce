using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record GetShopPageRequest(MerchantId Id);

/// <summary>
/// A Shop page's header: its name and description. Its Products come from
/// <see cref="ListStorefrontProductsEndpoint"/> with the Shop's ID.
/// </summary>
public class GetShopPageEndpoint(CatalogDbContext db) : Endpoint<GetShopPageRequest, ShopResponse>
{
    public override void Configure()
    {
        Get("/shops/{id:guid}");
        Group<Storefront>();
    }

    public override async Task HandleAsync(GetShopPageRequest req, CancellationToken ct)
    {
        var shop = await db.Shops.Where(s => s.Id == req.Id).ProjectToResponse().SingleOrDefaultAsync(ct);
        if (shop is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(shop, ct);
    }
}
