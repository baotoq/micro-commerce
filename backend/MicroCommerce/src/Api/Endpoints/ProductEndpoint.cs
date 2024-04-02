using Api.UseCases.Products;
using MediatR;

namespace Api.Endpoints;

public static class ProductEndpoint
{
    public static void MapProducts(
        this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/products")
            .WithTags("products");
        
        group.MapGet("/{id}", (IMediator mediator, string id) => mediator.Send(new GetProductQuery(id)));
        group.MapPost("/", (IMediator mediator, CreateProductCommand request) => mediator.Send(request));
    }
}