using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Carts;

public record CartItemId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CartItemId New() => new(Guid.NewGuid());
    public static CartItemId From(Guid value) => new(value);
}
