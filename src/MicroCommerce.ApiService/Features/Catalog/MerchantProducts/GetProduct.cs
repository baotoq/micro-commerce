using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

public sealed record ProductIdRequest(Guid Id);

/// <summary>One of the signed-in Merchant's own Products, in any status.</summary>
public sealed class GetMerchantProductEndpoint(MerchantCatalog catalog) : Endpoint<ProductIdRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id}");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "One of the Merchant's own Products";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
            s.Responses[404] = "No such Product among the Merchant's own.";
        });
    }

    public override async Task HandleAsync(ProductIdRequest req, CancellationToken ct)
    {
        var id = ProductId.From(req.Id);
        var product = await catalog.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}
