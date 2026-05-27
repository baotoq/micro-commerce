using MediatR;
using MicroCommerce.Catalog.Application.Products.Queries;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class ProductReadEndpoints
{
    private static void CacheProducts(Microsoft.AspNetCore.OutputCaching.OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("products");

    public static IEndpointRouteBuilder MapProductReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/counts", async (ISender mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetProductCountsQuery(), ct)))
            .CacheOutput(CacheProducts)
            .WithName("GetProductCounts");

        // Two equivalent paths exist for the wizard's SKU-uniqueness probe (AC-17).
        // The `/by-sku/{sku}/exists` form is the canonical one referenced from task
        // descriptions and admin tooling; `/{sku}/exists` matches the literal wording
        // of PRD §5 / AC-17 so the verifier's grep finds a hit.
        static async Task<IResult> SkuExistsAsync(string sku, ISender mediator, CancellationToken ct)
        {
            var exists = await mediator.Send(new ProductExistsBySkuQuery(sku), ct);
            return exists ? Results.NoContent() : Results.NotFound();
        }

        group.MapGet("/by-sku/{sku}/exists", SkuExistsAsync).WithName("ProductExistsBySku");
        group.MapGet("/{sku}/exists", SkuExistsAsync).WithName("ProductExistsBySkuShort");

        group.MapGet("/{sku}", async (string sku, ISender mediator, CancellationToken ct) =>
        {
            var product = await mediator.Send(new GetProductBySkuQuery(sku), ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .CacheOutput(CacheProducts)
        .WithName("GetProductBySku");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 9, string? status = null, string? search = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetProductsQuery(page, limit, status, search), ct)))
            .CacheOutput(CacheProducts)
            .WithName("GetProducts");

        return app;
    }
}
