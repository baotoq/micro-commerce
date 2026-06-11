using MediatR;
using MicroCommerce.Catalog.Application.Analytics.Queries;
using MicroCommerce.Catalog.Domain.Common;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class AnalyticsReadEndpoints
{
    private static void CacheAnalytics(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("analytics");

    private static void CacheDashboard(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("dashboard");

    public static IEndpointRouteBuilder MapAnalyticsReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics").WithTags("Analytics");

        group.MapGet("/overview", async (ISender mediator, IClock clock, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetAnalyticsOverviewQuery(clock), ct)))
            .CacheOutput(CacheAnalytics)
            .WithName("GetAnalyticsOverview");

        group.MapGet("/dashboard", async (ISender mediator, IClock clock, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetDashboardSummaryQuery(clock), ct)))
            .CacheOutput(CacheDashboard)
            .WithName("GetDashboardSummary");

        return app;
    }
}
