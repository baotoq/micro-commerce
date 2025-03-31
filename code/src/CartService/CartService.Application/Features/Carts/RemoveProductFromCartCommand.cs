using Ardalis.GuardClauses;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.CartService.Application.Features.Carts;

public record RemoveProductFromCartCommand : IRequest<Unit>
{
    public required CartId CartId { get; init; }
    public required CartItemId CartItemId { get; init; }
}

public class RemoveProductFromCartCommandHandler(ApplicationDbContext _context)
    : IRequestHandler<RemoveProductFromCartCommand, Unit>
{
    public async Task<Unit> Handle(RemoveProductFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

        if (cart is null)
        {
            throw new NotFoundException(request.CartId.ToString(), nameof(Cart));
        }

        cart.RemoveItem(request.CartItemId);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
