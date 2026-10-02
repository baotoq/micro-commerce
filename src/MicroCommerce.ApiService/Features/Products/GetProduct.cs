using System.Text.Json;
using FastEndpoints;
using MicroCommerce.ApiService.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;

namespace MicroCommerce.ApiService.Features.Products;

public record GetProductRequest(ProductId Id);

public class GetProductEndpoint(AppDbContext db, IDistributedCache cache)
    : Endpoint<GetProductRequest, ProductResponse>
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public override void Configure()
    {
        Get("/products/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var key = $"product:{req.Id}";

        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            HttpContext.Response.Headers["X-Cache"] = "HIT";
            await Send.OkAsync(JsonSerializer.Deserialize<ProductResponse>(cached)!, ct);
            return;
        }

        var product = await db.Products.FindAsync([req.Id], ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = ProductResponse.From(product);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), CacheOptions, ct);

        HttpContext.Response.Headers["X-Cache"] = "MISS";
        await Send.OkAsync(response, ct);
    }
}
