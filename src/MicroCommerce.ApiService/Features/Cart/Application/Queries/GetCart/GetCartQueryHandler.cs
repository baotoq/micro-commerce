using MediatR;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCart;

public sealed class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly CartDbContext _context;

    public GetCartQueryHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto?> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken);

        if (cart is null)
            return null;

        var items = cart.Items.Select(i => new CartItemDto(
            i.Id.Value,
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.ImageUrl,
            i.Quantity,
            LineTotal: i.UnitPrice * i.Quantity)).ToList();

        return new CartDto(
            cart.Id.Value,
            items,
            TotalPrice: items.Sum(i => i.LineTotal),
            TotalItems: items.Sum(i => i.Quantity));
    }
}
