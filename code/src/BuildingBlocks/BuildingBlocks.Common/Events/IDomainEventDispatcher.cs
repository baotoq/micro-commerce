namespace MicroCommerce.BuildingBlocks.Common.Events;

public interface IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent;
    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent;
}
