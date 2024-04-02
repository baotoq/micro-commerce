using Api.Carts;
using MediatR;

namespace Api.Endpoints;

public static class CartsEndpoint
{
    public static void MapCarts(
        this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/carts")
            .WithTags("carts");
        
        group.MapGet("/", (IMediator mediator) => mediator.Send(new GetCartQuery())).RequireAuthorization();
    }
}