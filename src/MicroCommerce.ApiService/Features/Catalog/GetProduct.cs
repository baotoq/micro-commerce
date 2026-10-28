using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record GetProductRequest(ProductId Id);

/// <summary>One of the Merchant's own Products, in any status; another Merchant's is not found.</summary>
public class GetProductEndpoint(MerchantCatalog catalog) : Endpoint<GetProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id:guid}");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var product = await catalog.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(product.ToResponse(), ct);
    }
}
