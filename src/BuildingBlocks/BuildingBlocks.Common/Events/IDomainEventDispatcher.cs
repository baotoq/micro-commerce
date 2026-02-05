namespace MicroCommerce.BuildingBlocks.Common.Events;

/// <summary>
/// In-process domain event dispatcher interface.
/// </summary>
/// <remarks>
/// OBSOLETE: This interface is deprecated. Use MassTransit with the transactional outbox pattern instead.
///
/// Migration guide:
/// 1. Remove IDomainEventDispatcher from your DI registrations
/// 2. Add MassTransit with EF Core outbox in Program.cs
/// 3. Use DomainEventInterceptor (SaveChangesInterceptor) to publish events after SaveChanges
/// 4. Events will be published via IPublishEndpoint to Azure Service Bus
///
/// Benefits of MassTransit outbox:
/// - At-least-once delivery guarantee
/// - Events survive application crashes
/// - Transactional consistency with aggregate changes
/// </remarks>
[Obsolete("Use MassTransit with transactional outbox pattern. See DomainEventInterceptor for new approach.")]
public interface IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent;
    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent;
}
