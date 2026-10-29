using FastEndpoints;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record GetStorefrontProductRequest(ProductId Id);

/// <summary>
/// A Product a Buyer can see, with every Variant; any other Product is not found. Served from
/// <see cref="StorefrontProductCache"/> when it can be, which the <c>X-Cache</c> header (<c>HIT</c> or <c>MISS</c>)
/// tells.
/// </summary>
public class GetStorefrontProductEndpoint(CatalogDbContext db, StorefrontProductCache cache)
    : Endpoint<GetStorefrontProductRequest, StorefrontProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id:guid}");
        Group<Storefront>();
    }

    public override async Task HandleAsync(GetStorefrontProductRequest req, CancellationToken ct)
    {
        if (await cache.GetAsync(req.Id, ct) is { } cached)
        {
            HttpContext.Response.Headers["X-Cache"] = "HIT";
            await Send.OkAsync(cached, ct);
            return;
        }

        var product = await db.VisibleProducts().AsNoTracking().SingleOrDefaultAsync(x => x.Product.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = product.ToStorefrontResponse()
            .WithAvailableStock(await db.ReservedQuantitiesAsync(product.Product.Id, ct));
        await cache.SetAsync(response, ct);

        HttpContext.Response.Headers["X-Cache"] = "MISS";
        await Send.OkAsync(response, ct);
    }
}
