using MediatR;
using MicroCommerce.Application.Products.Queries;

namespace MicroCommerce.Server.Endpoints;

public static class ProductReadEndpoints
{
    public static IEndpointRouteBuilder MapProductReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/counts", async (ISender mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetProductCountsQuery(), ct)))
            .WithName("GetProductCounts");

        group.MapGet("/{sku}", async (string sku, ISender mediator, CancellationToken ct) =>
        {
            var product = await mediator.Send(new GetProductBySkuQuery(sku), ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductBySku");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 9, string? status = null, string? search = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetProductsQuery(page, limit, status, search), ct)))
            .WithName("GetProducts");

        return app;
    }
}
