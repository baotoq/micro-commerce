using MicroCommerce.CartService.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace MicroCommerce.CartService.Application.Features;

[Mapper]
public partial class CartMapper
{
    public partial CartDto ToCartDto(Cart car);
}
