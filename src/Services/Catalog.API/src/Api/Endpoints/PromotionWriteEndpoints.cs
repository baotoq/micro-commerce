using MediatR;
using MicroCommerce.Catalog.Application.Promotions.Commands;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class PromotionWriteEndpoints
{
    public static IEndpointRouteBuilder MapPromotionWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/promotions").WithTags("Promotions");

        // TODO(auth): requireSeller() — endpoint currently unauthenticated; gate behind seller auth before going live.
        group.MapPost("/", async (CreatePromotionCommand command, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            if (result.IsFailure)
            {
                return Results.Problem(
                    title: result.Error!.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            var created = result.Value!;
            return Results.CreatedAtRoute("GetPromotionByCode", new { code = created.Code }, created);
        })
        .WithName("CreatePromotion");

        group.MapPut("/{code}", async (string code, UpdatePromotionCommand command, ISender mediator, CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { Code = code }, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .WithName("UpdatePromotion");

        group.MapDelete("/{code}", async (string code, ISender mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeletePromotionCommand(code), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeletePromotion");

        group.MapPost("/{code}/activate", async (string code, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivatePromotionCommand(code), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "PROMOTION_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("ActivatePromotion");

        group.MapPost("/{code}/end", async (string code, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new EndPromotionCommand(code), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "PROMOTION_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("EndPromotion");

        return app;
    }
}
