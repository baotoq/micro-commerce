using MediatR;
using MicroCommerce.Catalog.Application.Customers.Commands;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class CustomerWriteEndpoints
{
    public static IEndpointRouteBuilder MapCustomerWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers")
            .RequireAuthorization(AuthenticationExtensions.SellerPolicy);

        group.MapPost("/", async (CreateCustomerCommand command, ISender mediator, CancellationToken ct) =>
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
            return Results.CreatedAtRoute("GetCustomerByEmail", new { email = created.Email }, created);
        })
        .WithName("CreateCustomer");

        group.MapPost("/{email}/tags", async (string email, AddTagBody body, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new AddCustomerTagCommand(email, body.Tag), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "CUSTOMER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("AddCustomerTag");

        group.MapDelete("/{email}/tags/{tag}", async (string email, string tag, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveCustomerTagCommand(email, tag), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error!.Value.Code == "CUSTOMER_NOT_FOUND"
                ? Results.NotFound()
                : Results.Problem(
                    title: result.Error.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
        })
        .WithName("RemoveCustomerTag");

        return app;
    }

    private sealed record AddTagBody(string Tag);
}
