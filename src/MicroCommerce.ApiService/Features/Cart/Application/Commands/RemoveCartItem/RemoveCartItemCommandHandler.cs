using MediatR;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, Unit>
{
    private readonly CartDbContext _context;

    public RemoveCartItemCommandHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart not found for buyer '{request.BuyerId}'.");

        cart.RemoveItem(CartItemId.From(request.ItemId));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
