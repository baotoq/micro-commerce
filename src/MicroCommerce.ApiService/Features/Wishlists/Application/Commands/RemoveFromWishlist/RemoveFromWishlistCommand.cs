using MediatR;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Application.Commands.RemoveFromWishlist;

public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest;

public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly WishlistsDbContext _context;

    public RemoveFromWishlistCommandHandler(WishlistsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        RemoveFromWishlistCommand request,
        CancellationToken cancellationToken)
    {
        // Find item (idempotent behavior - no error if not found)
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(
                w => w.UserId == request.UserId && w.ProductId == request.ProductId,
                cancellationToken);

        if (item is null)
        {
            return;
        }

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
