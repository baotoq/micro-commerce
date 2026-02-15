namespace MicroCommerce.BuildingBlocks.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected init; } = default!;

    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }
}
