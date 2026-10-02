using FastEndpoints;
using MicroCommerce.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Products;

public class ListProductsEndpoint(AppDbContext db) : EndpointWithoutRequest<List<ProductResponse>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await db.Products
            .OrderByDescending(p => p.CreatedAt)
            .ProjectToResponse()
            .ToListAsync(ct);

        await Send.OkAsync(products, ct);
    }
}
