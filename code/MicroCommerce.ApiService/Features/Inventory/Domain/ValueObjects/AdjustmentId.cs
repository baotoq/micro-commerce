using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record AdjustmentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static AdjustmentId New() => new(Guid.NewGuid());
    public static AdjustmentId From(Guid value) => new(value);
}
