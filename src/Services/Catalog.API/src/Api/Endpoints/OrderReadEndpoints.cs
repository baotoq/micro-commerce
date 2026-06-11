using MediatR;
using MicroCommerce.Catalog.Application.Orders.Queries;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class OrderReadEndpoints
{
    private static void CacheOrders(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("orders");

    public static IEndpointRouteBuilder MapOrderReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/counts", async (ISender mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetOrderCountsQuery(), ct)))
            .CacheOutput(CacheOrders)
            .WithName("GetOrderCounts");

        group.MapGet("/{number:int}", async (int number, ISender mediator, CancellationToken ct) =>
        {
            var order = await mediator.Send(new GetOrderByNumberQuery(number), ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .CacheOutput(CacheOrders)
        .WithName("GetOrderByNumber");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 20, string? tab = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetOrdersQuery(page, limit, tab), ct)))
            .CacheOutput(CacheOrders)
            .WithName("GetOrders");

        return app;
    }
}
