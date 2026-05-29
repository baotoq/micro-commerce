using MediatR;
using MicroCommerce.Catalog.Application.Promotions.Queries;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class PromotionReadEndpoints
{
    private static void CachePromotions(OutputCachePolicyBuilder p) =>
        p.Expire(TimeSpan.FromMinutes(1)).Tag("promotions");

    public static IEndpointRouteBuilder MapPromotionReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/promotions").WithTags("Promotions");

        group.MapGet("/stats", async (ISender mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetPromotionStatsQuery(), ct)))
            .CacheOutput(CachePromotions)
            .WithName("GetPromotionStats");

        static async Task<IResult> ExistsByCodeAsync(string code, ISender mediator, CancellationToken ct)
        {
            var exists = await mediator.Send(new PromotionExistsByCodeQuery(code), ct);
            return exists ? Results.NoContent() : Results.NotFound();
        }

        group.MapGet("/by-code/{code}/exists", ExistsByCodeAsync).WithName("PromotionExistsByCode");
        group.MapGet("/{code}/exists", ExistsByCodeAsync).WithName("PromotionExistsByCodeShort");

        group.MapGet("/{code}", async (string code, ISender mediator, CancellationToken ct) =>
        {
            var promotion = await mediator.Send(new GetPromotionByCodeQuery(code), ct);
            return promotion is null ? Results.NotFound() : Results.Ok(promotion);
        })
        .CacheOutput(CachePromotions)
        .WithName("GetPromotionByCode");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 20, string? status = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(new GetPromotionsQuery(page, limit, status), ct)))
            .CacheOutput(CachePromotions)
            .WithName("GetPromotions");

        return app;
    }
}
