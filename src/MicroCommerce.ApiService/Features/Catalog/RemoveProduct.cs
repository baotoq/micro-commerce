using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record RemoveProductRequest(ProductId Id);

/// <summary>
/// Removes any Product from the Marketplace for good, whatever its Merchant or status: Buyers no longer find it and
/// its Merchant can no longer change it. Removing it again changes nothing.
/// </summary>
public class RemoveProductEndpoint(CatalogDbContext db, StorefrontProductCache storefrontCache)
    : Endpoint<RemoveProductRequest, OperatorProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id:guid}/remove");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(RemoveProductRequest req, CancellationToken ct)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        product.Remove();
        await db.SaveChangesAsync(ct);
        await storefrontCache.EvictAsync(product.Id, ct);

        await Send.OkAsync((await db.FindForOperatorAsync(product.Id, ct))!, ct);
    }
}
