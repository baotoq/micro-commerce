using MediatR;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetWishlistCount;

public sealed record GetWishlistCountQuery(Guid UserId) : IRequest<int>;

public sealed class GetWishlistCountQueryHandler : IRequestHandler<GetWishlistCountQuery, int>
{
    private readonly WishlistsDbContext _context;

    public GetWishlistCountQueryHandler(WishlistsDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        GetWishlistCountQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.WishlistItems
            .Where(w => w.UserId == request.UserId)
            .CountAsync(cancellationToken);
    }
}
