using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Queries;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class MarketingReadEndpoints
{
    private static void CacheMarketing(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("marketing");

    public static IEndpointRouteBuilder MapMarketingReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/marketing").WithTags("Marketing");

        // Frontend fetches the single draft via ?status=draft&limit=1 (plan OQ5).
        group.MapGet("/campaigns", async (ISender mediator,
            int page = 1, int limit = 20, string? status = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetCampaignsQuery(page, limit, status), ct)))
            .CacheOutput(CacheMarketing)
            .WithName("GetCampaigns");

        group.MapGet("/campaigns/{id:guid}", async (Guid id, ISender mediator, CancellationToken ct) =>
        {
            var draft = await mediator.Send(new GetMarketingDraftQuery(id), ct);
            return draft is null ? Results.NotFound() : Results.Ok(draft);
        })
        .CacheOutput(CacheMarketing)
        .WithName("GetMarketingDraft");

        return app;
    }
}
