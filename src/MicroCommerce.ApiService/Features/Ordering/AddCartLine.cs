using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Ordering;

public record AddCartLineRequest(VariantId VariantId, int Quantity);

public class AddCartLineValidator : Validator<AddCartLineRequest>
{
    public AddCartLineValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, Cart.MaxQuantity);
    }
}

/// <summary>
/// Adds a Variant to the signed-in Account's Cart, or adds to its quantity if the Cart already has it. Only a
/// Variant Buyers can see can be added (any other is not found); its stock is not checked.
/// </summary>
public class AddCartLineEndpoint(BuyerCart buyerCart, ICatalogLookup catalog) : CartEditEndpoint<AddCartLineRequest>(buyerCart)
{
    public override void Configure()
    {
        Post("/lines");
        Group<Carts>();
    }

    public override async Task HandleAsync(AddCartLineRequest req, CancellationToken ct)
    {
        var variants = await catalog.LookupAsync([req.VariantId], ct);
        if (!variants.TryGetValue(req.VariantId, out var variant) || !variant.IsBuyable)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var cart = await BuyerCart.GetForEditAsync(User, ct);
        cart.Add(req.VariantId, req.Quantity);
        await SaveAsync(cart, ct);
    }
}
