using FastEndpoints;

namespace MicroCommerce.ApiService.Features.Accounts;

public class GetShopEndpoint(CurrentMerchant currentMerchant) : EndpointWithoutRequest<MerchantResponse>
{
    public override void Configure()
    {
        Get("/shop");
        Group<MerchantCentre>();
    }

    public override Task HandleAsync(CancellationToken ct) =>
        Send.OkAsync(MerchantResponse.From(currentMerchant.Merchant), ct);
}
