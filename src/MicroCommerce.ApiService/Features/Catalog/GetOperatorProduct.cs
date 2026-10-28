using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record GetOperatorProductRequest(ProductId Id);

/// <summary>Any Product, whatever its Merchant or status, for a Platform Operator to review.</summary>
public class GetOperatorProductEndpoint(CatalogDbContext db) : Endpoint<GetOperatorProductRequest, OperatorProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id:guid}");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(GetOperatorProductRequest req, CancellationToken ct)
    {
        if (await db.FindForOperatorAsync(req.Id, ct) is { } product)
            await Send.OkAsync(product, ct);
        else
            await Send.NotFoundAsync(ct);
    }
}

internal static class OperatorProducts
{
    /// <returns>The Product with its Shop name, or null if no Product has the ID.</returns>
    public static async Task<OperatorProductResponse?> FindForOperatorAsync(this CatalogDbContext db, ProductId id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return null;

        var shopName = await db.Shops.Where(s => s.Id == product.MerchantId).Select(s => s.ShopName).SingleOrDefaultAsync(ct);
        return product.ToOperatorResponse(shopName);
    }
}
