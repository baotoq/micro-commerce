using MicroCommerce.CartService.Application.Features.Carts.Contracts;
using MicroCommerce.CartService.Domain.Cart;
using Riok.Mapperly.Abstractions;

namespace MicroCommerce.CartService.Application.Features.Carts.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CartMapper
{
    public static partial CartDto ToCartDto(Cart cart);
}
