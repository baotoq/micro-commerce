using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record OrderId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
}
