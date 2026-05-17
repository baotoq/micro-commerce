# Dapr Pub/Sub — Domain Events from Catalog.API

**Date:** 2026-05-17
**Status:** Approved

## Goal

Publish `ProductCreated`, `ProductUpdated`, and `ProductDeleted` domain events onto the Dapr pub/sub bus whenever the corresponding write commands succeed. No subscriber is wired yet; the bus is ready for future services (orders, search, notifications) to consume.

## Architecture

The MediatR notification pattern splits the concern across two layers:

```
Write endpoint
  → command handler (Application)
      → SaveChangesAsync
      → IPublisher.Publish(ProductXxxEvent)   // no Dapr here
          → INotificationHandler (Api)
              → DaprClient.PublishEventAsync  // Dapr only here
```

- **Application layer** owns the event record definitions and fires them via `IPublisher`. Zero Dapr imports.
- **Api layer** owns the notification handlers that call `DaprClient.PublishEventAsync`. Placed in `Api/EventHandlers/`.
- MediatR already scans the Application assembly. `Program.cs` adds a second `AddMediatR` call to also scan the Api assembly.

## AppHost Changes

```csharp
var pubSub = builder.AddDaprPubSub("pubsub");

var catalog = builder.AddProject<Projects.MicroCommerce_Catalog>("catalog-api")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar(sidecar => sidecar.WithReference(pubSub));
```

Default in-memory pub/sub component is sufficient for dev. Redis-backed configuration is deferred until a second subscriber service exists.

## Domain Events (Application layer)

Location: `Application/Products/Events/`

```csharp
record ProductCreatedEvent(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : INotification;
record ProductUpdatedEvent(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : INotification;
record ProductDeletedEvent(string Sku) : INotification;
```

## Handler Changes (Application layer)

Each handler gains `IPublisher publisher` as a constructor argument. After a successful `SaveChangesAsync`, publish the corresponding event:

| Handler | Event fired | Condition |
|---|---|---|
| `CreateProductHandler` | `ProductCreatedEvent` | always (after save) |
| `UpdateProductHandler` | `ProductUpdatedEvent` | only when product found and saved |
| `DeleteProductHandler` | `ProductDeletedEvent` | only when product found and deleted |

## Notification Handlers (Api layer)

Location: `Api/EventHandlers/`

Three thin classes — one per event — implementing `INotificationHandler<T>`:

```csharp
class ProductCreatedEventHandler(DaprClient dapr) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.created", e, ct);
}
```

Same pattern for `ProductUpdatedEventHandler` (`catalog.product.updated`) and `ProductDeletedEventHandler` (`catalog.product.deleted`).

## Topics

| Topic | Payload fields |
|---|---|
| `catalog.product.created` | Sku, Name, Category, Price, Inventory, Status |
| `catalog.product.updated` | Sku, Name, Category, Price, Inventory, Status |
| `catalog.product.deleted` | Sku |

Pub/sub component name: `"pubsub"` (matches Aspire resource name).

## Program.cs Changes

```csharp
// existing
builder.Services.AddApplication();

// add — scans Api assembly for INotificationHandler registrations
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

`AddDaprClient()` is already present; no change needed there.

## Testing

### Application.Tests (updated)

- All three command handler tests receive a mock `IPublisher` (e.g. `Substitute.For<IPublisher>()`).
- Success path: assert `publisher.Publish` was called with the correct event type and values.
- Not-found / conflict paths: assert `publisher.Publish` was **not** called.

### New unit tests for notification handlers

- One test class per handler.
- Mock `DaprClient`, fire the notification, assert `PublishEventAsync` called with correct pubsub name, topic, and payload.
- No I/O.

### Unchanged

- `FunctionalTests` and `IntegrationTests` — Dapr publishing is a side effect that does not change HTTP responses.

## Out of Scope

- Subscriber endpoints (`MapSubscribeHandler` / `[Topic]`) in Catalog.API.
- Transactional outbox — lost events on `PublishEventAsync` failure are accepted for now.
- Redis-backed pubsub component configuration.
- Dapr Workflow.
