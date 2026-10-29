using FastEndpoints;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Ordering;

public record RemoveCartLineRequest(VariantId VariantId);

/// <summary>Removes a Variant from the signed-in Account's Cart.</summary>
public class RemoveCartLineEndpoint(BuyerCart buyerCart) : CartEditEndpoint<RemoveCartLineRequest>(buyerCart)
{
    public override void Configure()
    {
        Delete("/lines/{variantId:guid}");
        Group<Carts>();
    }

    public override async Task HandleAsync(RemoveCartLineRequest req, CancellationToken ct)
    {
        var cart = await BuyerCart.GetForEditAsync(User, ct);
        if (!cart.Remove(req.VariantId))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await SaveAsync(cart, ct);
    }
}
