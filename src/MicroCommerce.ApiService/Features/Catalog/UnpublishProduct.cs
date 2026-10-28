using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>Moves one of the Merchant's Products back to Draft, hiding it from Buyers.</summary>
public class UnpublishProductEndpoint(MerchantCatalog catalog) : Endpoint<ChangeProductStatusRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id:guid}/unpublish");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(ChangeProductStatusRequest req, CancellationToken ct)
    {
        if (await catalog.ChangeStatusAsync(req.Id, p => p.Unpublish(), ct) is { } product)
            await Send.OkAsync(product.ToResponse(), ct);
        else
            await Send.NotFoundAsync(ct);
    }
}
