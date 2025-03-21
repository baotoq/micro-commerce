using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Entities;

namespace MicroCommerce.CartService.Domain.ValueObjects;

public record CartId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CartId New() => new(Guid.NewGuid());
    public static CartId From(Guid value) => new(value);
}
