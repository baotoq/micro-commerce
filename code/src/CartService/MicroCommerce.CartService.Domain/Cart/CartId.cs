using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Cart;

public record CartId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CartId New() => new(Guid.NewGuid());
    public static CartId From(Guid value) => new(value);
}
