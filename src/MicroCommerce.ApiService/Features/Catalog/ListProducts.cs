using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>Every one of the Merchant's own Products, in every status, newest first.</summary>
public class ListProductsEndpoint(MerchantCatalog catalog) : EndpointWithoutRequest<IReadOnlyList<ProductSummaryResponse>>
{
    public override void Configure()
    {
        Get("/products");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await catalog.Products
            .OrderByDescending(p => p.CreatedAt)
            .ProjectToSummaryResponse()
            .ToListAsync(ct);

        await Send.OkAsync(products, ct);
    }
}
