using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Queries;
using MicroCommerce.Catalog.Domain.Common;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class PayoutReadEndpoints
{
    private static void CachePayouts(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("payouts");

    public static IEndpointRouteBuilder MapPayoutReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payouts").WithTags("Payouts");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 20,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetPayoutsQuery(page, limit), ct)))
            .CacheOutput(CachePayouts)
            .WithName("GetPayouts");

        group.MapGet("/summary", async (ISender mediator, IClock clock, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetPayoutSummaryQuery(clock), ct)))
            .CacheOutput(CachePayouts)
            .WithName("GetPayoutSummary");

        group.MapGet("/ledger", async (ISender mediator,
            int page = 1, int limit = 20,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetLedgerQuery(page, limit), ct)))
            .CacheOutput(CachePayouts)
            .WithName("GetLedger");

        return app;
    }
}
