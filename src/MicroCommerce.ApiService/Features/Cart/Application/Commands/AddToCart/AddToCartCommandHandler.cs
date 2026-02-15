using MediatR;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;

public sealed class AddToCartCommandHandler
    : IRequestHandler<AddToCartCommand, Guid>
{
    private readonly CartDbContext _context;

    public AddToCartCommandHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        AddToCartCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken);

        if (cart is null)
        {
            cart = Domain.Entities.Cart.Create(request.BuyerId);
            _context.Carts.Add(cart);
        }

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.ImageUrl, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return cart.Id.Value;
    }
}
