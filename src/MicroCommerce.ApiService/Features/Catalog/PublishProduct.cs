using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Catalog;

public record ChangeProductStatusRequest(ProductId Id);

/// <summary>Publishes one of the Merchant's Products, so Buyers can find it; publishing it again changes nothing.</summary>
public class PublishProductEndpoint(MerchantCatalog catalog) : Endpoint<ChangeProductStatusRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id:guid}/publish");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(ChangeProductStatusRequest req, CancellationToken ct)
    {
        if (await catalog.ChangeStatusAsync(req.Id, p => p.Publish(), ct) is { } product)
            await Send.OkAsync(product.ToResponse(), ct);
        else
            await Send.NotFoundAsync(ct);
    }
}
