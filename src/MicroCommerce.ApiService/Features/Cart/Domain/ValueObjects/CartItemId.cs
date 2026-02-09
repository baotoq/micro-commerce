using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record CartItemId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CartItemId New() => new(Guid.NewGuid());
    public static CartItemId From(Guid value) => new(value);
}
