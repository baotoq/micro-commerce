using MicroCommerce.CartService.Domain.Cart;

namespace MicroCommerce.CartService.Application.Features.Carts.Contracts;

public record CartDto
{
    public required CartId Id { get; init; }
}
