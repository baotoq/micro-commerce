using MicroCommerce.CartService.Application.Features.Carts.Contracts;
using MicroCommerce.CartService.Domain.Cart;
using Riok.Mapperly.Abstractions;

namespace MicroCommerce.CartService.Application.Features.Carts.Mappers;

[Mapper]
public static partial class CartMapper
{
    public static partial CartDto ToCartDto(Cart cart);
}
