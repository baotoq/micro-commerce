using System.Diagnostics.CodeAnalysis;
using MediatR;
using MicroCommerce.CartService.Application.Features.Carts;
using MicroCommerce.CartService.Domain.Carts;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.CartService.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCarts(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/carts")
            .WithTags("Carts");

        group.MapGet("{id:guid}", async (IMediator mediator, [FromRoute] Guid id) =>
            await mediator.Send(new GetCartQuery
            {
                CartId = CartId.From(id)
            }))
            .WithName("GetCart")
            .Produces(200);

        group.MapPost("", async (IMediator mediator) =>
            await mediator.Send(new CreateCartCommand
            {
            }))
            .WithName("CreateCart")
            .Produces(200)
            .ProducesValidationProblem();

        group.MapDelete("{id:guid}/items/{itemId:guid}", async (IMediator mediator, [FromRoute] Guid id, [FromRoute] Guid itemId) =>
            await mediator.Send(new RemoveProductFromCartCommand
            {
                CartId = CartId.From(id),
                CartItemId = CartItemId.From(itemId)
            }))
            .WithName("RemoveProductFromCart")
            .Produces(200)
            .ProducesValidationProblem();
    }
}
