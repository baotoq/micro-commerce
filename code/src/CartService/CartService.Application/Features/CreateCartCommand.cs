using MediatR;
using MicroCommerce.CartService.Domain.Cart;
using MicroCommerce.CartService.Infrastructure;
using MicroCommerce.CartService.Infrastructure.Data;

namespace MicroCommerce.CartService.Application.Features;

public class CreateCartCommand : IRequest<CartDto>
{
}

public class CreateCartCommandHandler(ApplicationDbContext _context) : IRequestHandler<CreateCartCommand, CartDto>
{
    public async Task<CartDto> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var mapper = new CartMapper();
        var cart = new Cart(CartId.New());

        await _context.Carts.AddAsync(cart, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return mapper.ToCartDto(cart);
    }
}
