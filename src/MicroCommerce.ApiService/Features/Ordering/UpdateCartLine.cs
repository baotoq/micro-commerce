using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Ordering;

public record UpdateCartLineRequest(VariantId VariantId, int Quantity);

public class UpdateCartLineValidator : Validator<UpdateCartLineRequest>
{
    public UpdateCartLineValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, Cart.MaxQuantity);
    }
}

/// <summary>Sets the quantity of a Variant already in the signed-in Account's Cart.</summary>
public class UpdateCartLineEndpoint(BuyerCart buyerCart) : CartEditEndpoint<UpdateCartLineRequest>(buyerCart)
{
    public override void Configure()
    {
        Put("/lines/{variantId:guid}");
        Group<Carts>();
    }

    public override async Task HandleAsync(UpdateCartLineRequest req, CancellationToken ct)
    {
        var cart = await BuyerCart.GetForEditAsync(User, ct);
        if (!cart.SetQuantity(req.VariantId, req.Quantity))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await SaveAsync(cart, ct);
    }
}
