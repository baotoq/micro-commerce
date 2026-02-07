using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record ReservationId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ReservationId New() => new(Guid.NewGuid());
    public static ReservationId From(Guid value) => new(value);
}
