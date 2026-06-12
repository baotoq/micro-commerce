using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Commands;
using MicroCommerce.Catalog.Domain.Common;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class MarketingWriteEndpoints
{
    public static IEndpointRouteBuilder MapMarketingWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/marketing").WithTags("Marketing")
            .RequireAuthorization(AuthenticationExtensions.SellerPolicy);

        group.MapPost("/campaigns", async (CreateCampaignCommand command, IClock clock, ISender mediator, CancellationToken ct) =>
        {
            // The deserialized command has no live IClock; bind the registered demo clock here
            // (mirrors PromotionWriteEndpoints binding the route value into the command).
            var result = await mediator.Send(command with { Clock = clock }, ct);
            if (result.IsFailure)
            {
                return Results.Problem(
                    title: result.Error!.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            var created = result.Value!;
            return Results.CreatedAtRoute("GetMarketingDraft", new { id = created.Id }, created);
        })
        .WithName("CreateCampaign");

        group.MapPost("/campaigns/{id:guid}/schedule", async (Guid id, ScheduleCampaignCommand command, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { Id = id }, ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "CAMPAIGN_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("ScheduleCampaign");

        group.MapPost("/campaigns/{id:guid}/send", async (Guid id, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SendCampaignCommand(id), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "CAMPAIGN_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("SendCampaign");

        group.MapPost("/campaigns/{id:guid}/cancel", async (Guid id, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CancelCampaignCommand(id), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "CAMPAIGN_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("CancelCampaign");

        return app;
    }
}
