namespace MicroCommerce.BuildingBlocks.Common;

public abstract class BaseEntity<TId>(TId id)
{
    public TId Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
}
