using MediatR;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Photos;

namespace MicroCommerce.Catalog.Api.Endpoints;

public static class ProductWriteEndpoints
{
    public static IEndpointRouteBuilder MapProductWriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products")
            .RequireAuthorization(AuthenticationExtensions.SellerPolicy);

        group.MapPost("/", async (CreateProductCommand command, ISender mediator, CancellationToken ct) =>
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
            return Results.CreatedAtRoute("GetProductBySku", new { sku = created.Sku }, created);
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

        // TODO(security): rate-limit POST /api/products/photo-upload-url to prevent
        // SAS-token enumeration / abuse. Aspire has no rate-limit primitive yet; consider
        // AspNetCoreRateLimit or a Dapr middleware.
        group.MapPost("/photo-upload-url", async (PhotoUploadUrlCommand command, ISender mediator, CancellationToken ct) =>
        {
            var response = await mediator.Send(command, ct);
            return Results.Ok(response);
        })
        .WithName("RequestProductPhotoUploadUrl");

        return app;
    }
}
