using MediatR;
using MicroCommerce.Catalog.Application.Customers.Queries;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class CustomerReadEndpoints
{
    private static void CacheCustomers(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("customers");

    public static IEndpointRouteBuilder MapCustomerReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 20, string? search = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetCustomersQuery(page, limit, search), ct)))
            .CacheOutput(CacheCustomers)
            .WithName("GetCustomers");

        group.MapGet("/{email}", async (string email, ISender mediator, CancellationToken ct) =>
        {
            var customer = await mediator.Send(new GetCustomerByEmailQuery(email), ct);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        })
        .CacheOutput(CacheCustomers)
        .WithName("GetCustomerByEmail");

        return app;
    }
}
