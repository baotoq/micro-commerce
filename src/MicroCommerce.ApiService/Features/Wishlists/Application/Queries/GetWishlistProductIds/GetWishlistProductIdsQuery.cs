using MediatR;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetWishlistProductIds;

public sealed record GetWishlistProductIdsQuery(Guid UserId) : IRequest<List<Guid>>;

public sealed class GetWishlistProductIdsQueryHandler
    : IRequestHandler<GetWishlistProductIdsQuery, List<Guid>>
{
    private readonly WishlistsDbContext _context;

    public GetWishlistProductIdsQueryHandler(WishlistsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guid>> Handle(
        GetWishlistProductIdsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.WishlistItems
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);
    }
}
