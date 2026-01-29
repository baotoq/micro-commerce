using MediatR;

namespace MicroCommerce.BuildingBlocks.Common.Events;

/// <summary>
/// In-process domain event handler interface using MediatR.
/// </summary>
/// <remarks>
/// OBSOLETE: This interface is deprecated. Use MassTransit IConsumer instead for reliable event handling.
///
/// Migration guide:
/// 1. Create a MassTransit consumer that implements IConsumer&lt;TDomainEvent&gt;
/// 2. Register consumers with x.AddConsumers(assembly) in MassTransit configuration
/// 3. Consumers automatically receive events from Azure Service Bus
///
/// Example consumer:
/// <code>
/// public class ProductCreatedConsumer : IConsumer&lt;ProductCreatedEvent&gt;
/// {
///     public async Task Consume(ConsumeContext&lt;ProductCreatedEvent&gt; context)
///     {
///         var @event = context.Message;
///         // Handle the event
///     }
/// }
/// </code>
///
/// Benefits of MassTransit consumers:
/// - Automatic retry with configurable policies
/// - Dead-letter queue for failed messages
/// - Message correlation and tracking
/// - Concurrency control
/// </remarks>
[Obsolete("Use MassTransit IConsumer<TDomainEvent> instead for reliable event handling with retry support.")]
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
    new Task Handle(TDomainEvent @event, CancellationToken cancellationToken);
}
