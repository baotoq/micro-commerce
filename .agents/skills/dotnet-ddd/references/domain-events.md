# Domain Events

Domain Events capture **something that happened** in the domain that other parts of the system care about.

## Core Interface

```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
```

## Defining Events

Events are **past-tense facts** — immutable records of what happened:

```csharp
public record OrderPlacedEvent(OrderId OrderId, CustomerId CustomerId, Money Total) : DomainEvent;
public record OrderCancelledEvent(OrderId OrderId, string Reason) : DomainEvent;
public record PaymentReceivedEvent(PaymentId PaymentId, OrderId OrderId, Money Amount) : DomainEvent;
```

**Naming convention:** `{Noun}{PastTenseVerb}Event` — e.g., `OrderPlaced`, `UserRegistered`, `ItemAddedToCart`.

## Raising Events

Events are collected in the Entity base class and dispatched after persistence:

```csharp
// Inside an Aggregate method
public void Cancel(string reason)
{
    if (Status == OrderStatus.Shipped)
        throw new DomainException("Cannot cancel shipped orders");

    Status = OrderStatus.Cancelled;
    RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
}
```

## Dispatching Strategies

### Strategy 1: Dispatch After SaveChanges (Recommended)

Dispatch domain events after the transaction commits. This is the safest approach — events only fire when state is persisted.

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}

// In your Unit of Work or DbContext SaveChanges override:
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    // Collect events before saving (entities might get detached)
    var events = ChangeTracker.Entries<Entity<object>>()
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    // Save first
    var result = await base.SaveChangesAsync(ct);

    // Dispatch after successful save
    await _dispatcher.DispatchAsync(events, ct);

    // Clear events
    foreach (var entry in ChangeTracker.Entries<Entity<object>>())
        entry.Entity.ClearDomainEvents();

    return result;
}
```

### Strategy 2: Dispatch Before SaveChanges

Useful when handlers need to modify state within the same transaction:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    // Dispatch BEFORE saving — handlers can modify entities
    await DispatchDomainEventsAsync(ct);

    return await base.SaveChangesAsync(ct);
}

private async Task DispatchDomainEventsAsync(CancellationToken ct)
{
    // Loop until no more events (handlers may raise new events)
    while (true)
    {
        var events = ChangeTracker.Entries<Entity<object>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        if (events.Count == 0) break;

        foreach (var entry in ChangeTracker.Entries<Entity<object>>())
            entry.Entity.ClearDomainEvents();

        await _dispatcher.DispatchAsync(events, ct);
    }
}
```

### Strategy 3: MediatR-Based Dispatcher

A common implementation using MediatR (or any mediator):

```csharp
public sealed class MediatRDomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            await mediator.Publish(domainEvent, ct);
        }
    }
}
```

Make domain events implement `INotification`:

```csharp
public abstract record DomainEvent : IDomainEvent, INotification
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
```

### Strategy 4: Simple In-Process Dispatcher (No Dependencies)

```csharp
public sealed class InProcessDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InProcessDomainEventDispatcher(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (dynamic handler in handlers)
            {
                await handler.HandleAsync((dynamic)domainEvent, ct);
            }
        }
    }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
```

## Domain Events vs Integration Events

| Aspect | Domain Event | Integration Event |
|---|---|---|
| **Scope** | Within a Bounded Context | Across Bounded Contexts / services |
| **Transport** | In-process (mediator) | Message broker (RabbitMQ, Kafka, etc.) |
| **Consistency** | Same transaction (or immediate) | Eventually consistent |
| **Naming** | `OrderPlacedEvent` | `OrderPlacedIntegrationEvent` |
| **Schema** | Can use domain types | Should use primitive types (serializable) |

### Integration Event Example

```csharp
// Integration events use primitive types — no domain types
public record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime PlacedAt);
```

### Converting Domain Event to Integration Event

```csharp
public sealed class OrderPlacedEventHandler(IIntegrationEventPublisher publisher)
    : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
    {
        var integrationEvent = new OrderPlacedIntegrationEvent(
            domainEvent.OrderId.Value,
            domainEvent.CustomerId.Value,
            domainEvent.Total.Amount,
            domainEvent.Total.Currency,
            domainEvent.OccurredOn);

        await publisher.PublishAsync(integrationEvent, ct);
    }
}
```

## Outbox Pattern

For reliable event publishing (guarantees events are eventually published even if the broker is down):

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;  // JSON
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
```

**How it works:**
1. Within the same transaction as the domain change, write integration events to an `OutboxMessages` table
2. A background worker polls the table, publishes events to the broker, marks them processed
3. This guarantees at-least-once delivery

```csharp
// In SaveChangesAsync — same transaction as entity changes
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var events = CollectDomainEvents();
    ConvertToOutboxMessages(events);  // Adds OutboxMessage entities
    return await base.SaveChangesAsync(ct);
}

// Background worker
public sealed class OutboxProcessor(AppDbContext db, IMessageBroker broker) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var messages = await db.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                await broker.PublishAsync(message.Type, message.Payload, ct);
                message.ProcessedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

## Guidelines

- Raise events for business-significant state changes, not for every property update
- Keep event payloads small — include IDs and essential data, not full objects
- Domain events are part of the domain model — define them in the Domain layer
- Integration events are part of the Application layer
- Never handle domain events with infrastructure concerns directly — use integration events for cross-boundary communication
