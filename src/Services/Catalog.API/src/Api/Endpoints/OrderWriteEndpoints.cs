using MediatR;
using MicroCommerce.Catalog.Application.Orders.Commands;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class OrderWriteEndpoints
{
    public static IEndpointRouteBuilder MapOrderWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders")
            .RequireAuthorization(AuthenticationExtensions.SellerPolicy);

        // TODO(buyer-checkout): in production orders originate from a buyer checkout flow, not a seller POST (OQ7).
        group.MapPost("/", async (CreateOrderCommand command, ISender mediator, CancellationToken ct) =>
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
            return Results.CreatedAtRoute("GetOrderByNumber", new { number = created.Number }, created);
        })
        .WithName("CreateOrder");

        group.MapPost("/{number:int}/pack", async (int number, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new PackOrderCommand(number), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "ORDER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("PackOrder");

        group.MapPost("/{number:int}/ship", async (int number, ShipOrderCommand command, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { Number = number }, ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "ORDER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("ShipOrder");

        group.MapPost("/{number:int}/deliver", async (int number, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeliverOrderCommand(number), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "ORDER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("DeliverOrder");

        group.MapPost("/{number:int}/cancel", async (int number, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CancelOrderCommand(number), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "ORDER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("CancelOrder");

        group.MapPut("/{number:int}/note", async (int number, UpdateOrderNoteCommand command, ISender mediator, CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { Number = number }, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .WithName("UpdateOrderNote");

        return app;
    }
}
