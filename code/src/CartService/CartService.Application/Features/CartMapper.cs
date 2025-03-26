using MicroCommerce.CartService.Domain.Cart;
using Riok.Mapperly.Abstractions;

namespace MicroCommerce.CartService.Application.Features;

[Mapper]
public partial class CartMapper
{
#pragma warning disable RMG020
    public partial CartDto ToCartDto(Cart cart);
#pragma warning restore RMG020
}
