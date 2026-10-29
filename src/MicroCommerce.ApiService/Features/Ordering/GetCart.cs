using FastEndpoints;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>The signed-in Account's Cart, each line read live and flagged when it can't be checked out as it is.</summary>
public class GetCartEndpoint(BuyerCart buyerCart) : EndpointWithoutRequest<CartResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<Carts>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cart = await buyerCart.GetAsync(User, ct);
        await Send.OkAsync(await buyerCart.ViewAsync(cart, User, ct), ct);
    }
}
