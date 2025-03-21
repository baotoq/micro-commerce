using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Entities;

namespace MicroCommerce.CartService.Domain.ValueObjects;

public record CartId(Guid Value) : ValueGuidObjectId<Cart>(Value)
{
    public static CartId New() => new(Guid.CreateVersion7());
    public static CartId From(Guid value) => new(value);
}
