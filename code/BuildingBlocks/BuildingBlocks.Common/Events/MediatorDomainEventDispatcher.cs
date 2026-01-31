using MediatR;

namespace MicroCommerce.BuildingBlocks.Common.Events;

/// <summary>
/// In-process domain event dispatcher using MediatR.
/// </summary>
/// <remarks>
/// OBSOLETE: This dispatcher is deprecated. Use MassTransit with the transactional outbox pattern instead.
///
/// Problems with in-process dispatch:
/// - No delivery guarantee if application crashes after database commit
/// - Events lost on process termination
/// - No retry mechanism for failed handlers
///
/// Migration guide:
/// 1. Remove MediatorDomainEventDispatcher registration from DI
/// 2. Configure MassTransit with EF Core outbox in Program.cs
/// 3. Add DomainEventInterceptor to your DbContext
/// 4. Domain events from aggregates will be automatically published via outbox
///
/// Example MassTransit configuration:
/// <code>
/// builder.Services.AddMassTransit(x =>
/// {
///     x.AddEntityFrameworkOutbox&lt;OutboxDbContext&gt;(o =>
///     {
///         o.UsePostgres();
///         o.UseBusOutbox();
///     });
///     x.UsingAzureServiceBus((context, cfg) =>
///     {
///         cfg.Host(connectionString);
///         cfg.ConfigureEndpoints(context);
///     });
/// });
/// </code>
/// </remarks>
[Obsolete("Use MassTransit with transactional outbox pattern. See DomainEventInterceptor for new approach.")]
public class MediatorDomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent
    {
        return Task.WhenAll(domainEvents.Select(e => mediator.Publish(e)));
    }

    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent
    {
        return mediator.Publish(domainEvent);
    }
}
