using Ardalis.GuardClauses;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Infrastructure.Data;

namespace MicroCommerce.CartService.Application.Features.Carts;

public record AddProductToCartCommand : IRequest<CartId>
{
    public required CartId CartId { get; init; }
    public required CartItemId CartItemId { get; init; }
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

        cart.AddItem(request.CartItemId, request.Quantity, new Money(0));

        await _context.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
