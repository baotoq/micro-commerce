using MediatR;
using MicroCommerce.ApiService.Features.Wishlists.Domain.Entities;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Application.Commands.AddToWishlist;

public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Guid>;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Guid>
{
    private readonly WishlistsDbContext _context;

    public AddToWishlistCommandHandler(WishlistsDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        AddToWishlistCommand request,
        CancellationToken cancellationToken)
    {
        // Check if already in wishlist (idempotent behavior)
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(
                w => w.UserId == request.UserId && w.ProductId == request.ProductId,
                cancellationToken);

        if (existing is not null)
        {
            return existing.Id.Value;
        }

        // Create new wishlist item
        var item = WishlistItem.Create(request.UserId, request.ProductId);
        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item.Id.Value;
    }
}
