using System;
using MediatR;
using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;
using MicroCommerce.CartService.Application.Exceptions;
using Ardalis.GuardClauses;

namespace MicroCommerce.CartService.Application.Features;

public class AddProductToCartCommand : IRequest<CartId>
{
    public required CartId CartId { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}

public class AddProductToCartCommandHandler : IRequestHandler<AddProductToCartCommand, CartId>
{
    public async Task<CartId> Handle(AddProductToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = new Cart(request.CartId);
        if (cart == null)
        {
            throw new NotFoundException(request.CartId.ToString(), nameof(Cart));
        }

        cart.AddItem(request.ProductId, request.Quantity, new Price(0));

        return cart.Id;
    }
}
