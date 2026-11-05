using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

public sealed record ShopIdRequest(Guid Id);

/// <summary>A Merchant's public Shop page header; its Products come from the list filtered by shopId (FR-031).</summary>
public sealed class GetShopPageEndpoint(IMerchantDirectory merchants) : Endpoint<ShopIdRequest, ShopPageResponse>
{
    public override void Configure()
    {
        Get("/shops/{id}");
        Group<MarketplaceGroup>();
        Summary(s =>
        {
            s.Summary = "A Shop page";
            s.Responses[404] = "No such Shop.";
        });
    }

    public override async Task HandleAsync(ShopIdRequest req, CancellationToken ct)
    {
        var id = MerchantId.From(req.Id);
        var shops = await merchants.GetShopsAsync([id], ct);
        if (!shops.TryGetValue(id, out var shop))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new ShopPageResponse(shop.Id.Value, shop.ShopName, shop.Description), ct);
    }
}
