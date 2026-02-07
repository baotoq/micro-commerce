using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record OrderItemId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static OrderItemId New() => new(Guid.NewGuid());
    public static OrderItemId From(Guid value) => new(value);
}
