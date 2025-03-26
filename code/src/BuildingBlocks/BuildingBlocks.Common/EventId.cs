namespace MicroCommerce.BuildingBlocks.Common;

public record EventId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static EventId New() => new(Guid.NewGuid());
    public static EventId From(Guid value) => new(value);
}
