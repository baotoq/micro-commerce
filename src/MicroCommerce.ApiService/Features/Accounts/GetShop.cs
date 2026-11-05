using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>The signed-in Merchant's own Shop profile, Pickup Address included.</summary>
public sealed class GetShopEndpoint(ICurrentMerchant currentMerchant, AccountsDbContext db)
    : EndpointWithoutRequest<ShopProfileResponse>
{
    public override void Configure()
    {
        Get("/shop");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "The signed-in Merchant's Shop profile";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchant = await db.Merchants.AsNoTracking().SingleAsync(m => m.Id == currentMerchant.Id, ct);
        await Send.OkAsync(ShopProfileResponse.From(merchant), ct);
    }
}
