using MediatR;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCartItemCount;

public sealed class GetCartItemCountQueryHandler
    : IRequestHandler<GetCartItemCountQuery, int>
{
    private readonly CartDbContext _context;

    public GetCartItemCountQueryHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        GetCartItemCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await _context.Carts
            .AsNoTracking()
            .Where(c => c.BuyerId == request.BuyerId)
            .SelectMany(c => c.Items)
            .SumAsync(i => i.Quantity, cancellationToken);

        return count;
    }
}
