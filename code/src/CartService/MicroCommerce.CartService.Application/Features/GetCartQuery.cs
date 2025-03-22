using MediatR;
using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Application.Features;

public class GetCartQuery : IRequest<CartDto>
{

}

public record CartDto
{
    public required CartId Id { get; init; }
}

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    public Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var mapper = new CartMapper();
        var cart = new Cart(CartId.New());

        return Task.FromResult(mapper.ToCartDto(cart));
    }
}
