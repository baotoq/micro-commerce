using MediatR;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler
    : IRequestHandler<UpdateCartItemCommand, Unit>
{
    private readonly CartDbContext _context;

    public UpdateCartItemCommandHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        UpdateCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart not found for buyer '{request.BuyerId}'.");

        cart.UpdateItemQuantity(CartItemId.From(request.ItemId), request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
