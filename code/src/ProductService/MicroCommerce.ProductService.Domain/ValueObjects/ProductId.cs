using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ProductService.Domain.ValueObjects;

public record ProductId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId From(Guid value) => new(value);
}
