using MicroCommerce.CartService.Application.Features.Carts.Contracts;
using MicroCommerce.CartService.Application.Features.Carts.Mappers;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Infrastructure.Data;

namespace MicroCommerce.CartService.Application.Features.Carts;

public record CreateCartCommand : IRequest<CartDto>
{
}

public class CreateCartCommandHandler(ApplicationDbContext _context) : IRequestHandler<CreateCartCommand, CartDto>
{
    public async Task<CartDto> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = new Cart(CartId.New());

        await _context.Carts.AddAsync(cart, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return CartMapper.ToCartDto(cart);
    }
}
