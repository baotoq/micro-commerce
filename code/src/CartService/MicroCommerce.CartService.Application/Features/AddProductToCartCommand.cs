using System;
using MediatR;
using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;
using MicroCommerce.CartService.Application.Exceptions;
using Ardalis.GuardClauses;
using MicroCommerce.CartService.Infrastructure;

namespace MicroCommerce.CartService.Application.Features;

public class AddProductToCartCommand : IRequest<CartId>
{
    public required CartId CartId { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}

public class AddProductToCartCommandHandler(ApplicationDbContext _context)
    : IRequestHandler<AddProductToCartCommand, CartId>
{
    public async Task<CartId> Handle(AddProductToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = _context.Carts.FirstOrDefault(c => c.Id == request.CartId);
        if (cart == null)
        {
            throw new NotFoundException(request.CartId.ToString(), nameof(Cart));
        }

        cart.AddItem(request.ProductId, request.Quantity, new Price(0));

        await _context.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
