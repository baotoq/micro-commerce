using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ProductService.Domain.ValueObjects;

public record ProductId(Guid Value) : ValueGuidObjectId<Guid>(Value)
{
    public static ProductId New() => new(Guid.CreateVersion7());
    public static ProductId From(Guid value) => new(value);
}
