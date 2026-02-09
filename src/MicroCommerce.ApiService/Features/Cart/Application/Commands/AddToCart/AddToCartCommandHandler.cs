using MediatR;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;

public sealed class AddToCartCommandHandler
    : IRequestHandler<AddToCartCommand, AddToCartResult>
{
    private readonly CartDbContext _context;

    public AddToCartCommandHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task<AddToCartResult> Handle(
        AddToCartCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken);

        var isUpdate = false;

        if (cart is null)
        {
            cart = Domain.Entities.Cart.Create(request.BuyerId);
            _context.Carts.Add(cart);
        }
        else
        {
            // Check if product already exists in cart (will increment quantity)
            isUpdate = cart.Items.Any(i => i.ProductId == request.ProductId);
        }

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.ImageUrl, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return new AddToCartResult(isUpdate);
    }
}
