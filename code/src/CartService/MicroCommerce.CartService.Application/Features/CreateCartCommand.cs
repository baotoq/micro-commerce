using MediatR;
using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Application.Features;

public class CreateCartCommand : IRequest<CartDto>
{
}

public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CartDto>
{
    public async Task<CartDto> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var mapper = new CartMapper();
        var cart = new Cart(CartId.New());

        return mapper.ToCartDto(cart);
    }
}
