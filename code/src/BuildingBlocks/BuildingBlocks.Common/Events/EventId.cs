using System.Diagnostics;

namespace MicroCommerce.BuildingBlocks.Common.Events;

[DebuggerStepThrough]
public record EventId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static EventId New() => new(Guid.NewGuid());
    public static EventId From(Guid value) => new(value);
}
