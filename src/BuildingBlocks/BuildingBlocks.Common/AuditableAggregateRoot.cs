namespace MicroCommerce.BuildingBlocks.Common;

public abstract class AuditableAggregateRoot<TId> : BaseAggregateRoot<TId>, IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    protected AuditableAggregateRoot() : base()
    {
    }

    protected AuditableAggregateRoot(TId id) : base(id)
    {
    }
}
