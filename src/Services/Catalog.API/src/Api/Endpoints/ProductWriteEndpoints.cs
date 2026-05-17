using MediatR;
using MicroCommerce.Catalog.Application.Products.Commands;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class ProductWriteEndpoints
{
    public static IEndpointRouteBuilder MapProductWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapPost("/", async (CreateProductCommand command, ISender mediator, CancellationToken ct) =>
        {
            try
            {
                var created = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetProductBySku", new { sku = created.Sku }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
        })
        .WithName("CreateProduct");

        group.MapPut("/{sku}", async (string sku, UpdateProductCommand command, ISender mediator, CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { Sku = sku }, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .WithName("UpdateProduct");

        group.MapDelete("/{sku}", async (string sku, ISender mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteProductCommand(sku), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct");

        return app;
    }
}
