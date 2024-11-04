using MediatR;
using MicroCommerce.ApiService.UseCases.Carts;

namespace MicroCommerce.ApiService.Endpoints;

public static class CartEndpoint
{
    public static void MapCarts(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/carts")
            .WithTags("carts");
        
        group.MapGet("/{id}", (IMediator mediator, string id) => mediator.Send(new GetCartQuery(id)));
        group.MapPost("/products/increase", (IMediator mediator, IncreaseProductQuantityCommand request) => mediator.Send(request));
    }
}