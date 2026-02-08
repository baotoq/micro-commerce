# Phase 5: Event Bus Infrastructure - Research

**Researched:** 2026-02-08
**Domain:** MassTransit messaging hardening (idempotency, DLQ, correlation, error handling)
**Confidence:** HIGH

## Summary

This phase hardens the existing MassTransit + Azure Service Bus + EF Core outbox infrastructure established in Phase 1. The project already has MassTransit 9.0.0 with Azure Service Bus transport, an EF Core transactional outbox (`OutboxDbContext` with `InboxState`, `OutboxMessage`, `OutboxState` entities), and one consumer (`ProductCreatedConsumer` in the Inventory module).

The core approach uses MassTransit's built-in Consumer Outbox (inbox + outbox pattern) for message deduplication, applied globally via `AddConfigureEndpointsCallback`. For error handling, MassTransit provides `UseMessageRetry` with exponential backoff and `UseCircuitBreaker` as middleware filters. Azure Service Bus has a native dead-letter sub-queue (`/$deadletterqueue`) that MassTransit can route faulted messages to via `ConfigureDeadLetterQueueErrorTransport()`. For OpenTelemetry/distributed tracing, MassTransit natively supports `System.Diagnostics.Activity` and propagates trace context across message boundaries via headers -- the Aspire service defaults already configure OTLP export, so MassTransit traces will appear in the Aspire dashboard by adding its `ActivitySource`.

**Primary recommendation:** Use `UseEntityFrameworkOutbox` on all consumer endpoints for inbox-based deduplication, `UseMessageRetry` with exponential intervals for retries, `UseCircuitBreaker` for fault isolation, and route errors to Azure Service Bus native DLQ. Build a lightweight admin DLQ page using the Azure Service Bus SDK to browse/retry/purge dead-lettered messages.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| MassTransit | 9.0.0 | Message bus abstraction | Already in project; provides retry, circuit breaker, outbox, OpenTelemetry natively |
| MassTransit.Azure.ServiceBus.Core | 9.0.0 | Azure Service Bus transport | Already in project; has DLQ routing support |
| MassTransit.EntityFrameworkCore | 9.0.0 | EF Core outbox + inbox | Already in project; inbox provides message deduplication |
| Azure.Messaging.ServiceBus | (Aspire managed) | Direct DLQ access | Required for admin DLQ page to browse/retry/purge dead-lettered messages |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| OpenTelemetry (via Aspire ServiceDefaults) | (already configured) | Distributed tracing | MassTransit auto-instruments; just add ActivitySource |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| EF Core inbox deduplication | Manual idempotency checks per consumer | More code, error-prone, inconsistent |
| Azure Service Bus native DLQ | MassTransit `_error` queue | Native DLQ integrates with Azure tooling, but `_error` queue is MassTransit default |
| Direct Azure SDK for DLQ admin | ServicePulse | ServicePulse is a paid third-party tool; direct SDK is free and fits the project |

**Installation:**
```bash
# Azure.Messaging.ServiceBus needed for DLQ admin page API endpoints
# Check if Aspire already provides this transitively; if not:
dotnet add code/MicroCommerce.ApiService package Azure.Messaging.ServiceBus
```

## Architecture Patterns

### Recommended Project Structure
```
MicroCommerce.ApiService/
├── Common/
│   ├── Messaging/
│   │   ├── ConsumerConfiguration.cs          # Global retry, circuit breaker, outbox config
│   │   ├── DeadLetterQueueService.cs         # Browse/retry/purge DLQ via Azure SDK
│   │   └── Exceptions/
│   │       └── PermanentException.cs         # Marker for non-retryable errors
│   └── Persistence/
│       └── OutboxDbContext.cs                # (existing) Already has inbox entities
├── Features/
│   ├── Catalog/Domain/Events/               # (existing) Domain events
│   ├── Inventory/Application/Consumers/
│   │   └── ProductCreatedConsumer.cs         # (existing) Retrofit with structured logging
│   └── Messaging/                            # Admin DLQ management
│       ├── Application/
│       │   ├── GetDeadLetterMessagesQuery.cs
│       │   ├── RetryDeadLetterMessageCommand.cs
│       │   └── PurgeDeadLetterMessagesCommand.cs
│       └── DeadLetterEndpoints.cs            # API endpoints for DLQ admin
```

### Pattern 1: Global Consumer Middleware via AddConfigureEndpointsCallback
**What:** Apply retry, circuit breaker, and EF Core outbox (inbox) to ALL consumers centrally
**When to use:** Default for all consumers; per-consumer overrides via ConsumerDefinition
**Example:**
```csharp
// Source: https://masstransit.io/documentation/configuration/middleware/outbox
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.AddEntityFrameworkOutbox<OutboxDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        // Circuit breaker - stop consuming after repeated failures
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;   // 15% failure rate trips
            cb.ActiveThreshold = 10; // Min 10 messages before evaluating
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // Retry with exponential backoff
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(3,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(5));
            r.Ignore<PermanentException>();
        });

        // EF Core outbox on consumer side = inbox deduplication
        cfg.UseEntityFrameworkOutbox<OutboxDbContext>(context);
    });

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));

        // Route faulted messages to Azure Service Bus native DLQ
        if (cfg is IServiceBusReceiveEndpointConfigurator sb)
        {
            sb.ConfigureDeadLetterQueueErrorTransport();
        }

        cfg.ConfigureEndpoints(context);
    });
});
```

### Pattern 2: Per-Consumer Override via ConsumerDefinition
**What:** Override default retry/middleware for specific consumers
**When to use:** When a consumer needs different retry counts or circuit breaker thresholds
**Example:**
```csharp
// Source: https://masstransit.io/documentation/configuration/consumers
public class ProductCreatedConsumerDefinition :
    ConsumerDefinition<ProductCreatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductCreatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Override: more retries for this specific consumer
        endpointConfigurator.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(25),
            TimeSpan.FromSeconds(60)));
    }
}
```

### Pattern 3: Transient vs Permanent Error Classification
**What:** Route permanent errors directly to DLQ; retry only transient errors
**When to use:** Always -- prevents wasting retries on errors that will never succeed
**Example:**
```csharp
// Marker exception for non-retryable errors
public class PermanentException : Exception
{
    public PermanentException(string message) : base(message) { }
    public PermanentException(string message, Exception inner) : base(message, inner) { }
}

// In retry configuration:
cfg.UseMessageRetry(r =>
{
    r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));
    r.Ignore<PermanentException>();        // Skip retries, go straight to error/DLQ
    r.Ignore<ValidationException>();       // Business rule violations won't fix themselves
    r.Ignore<InvalidOperationException>(); // Logic errors won't fix themselves
});
```

### Pattern 4: DLQ Access via Azure Service Bus SDK
**What:** Browse, retry, and purge dead-lettered messages using ServiceBusReceiver
**When to use:** Admin DLQ page API endpoints
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
public class DeadLetterQueueService
{
    private readonly ServiceBusClient _client;

    // Browse DLQ messages (peek without consuming)
    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekDeadLettersAsync(
        string queueName, int maxMessages = 20)
    {
        var dlqPath = $"{queueName}/$deadletterqueue";
        var receiver = _client.CreateReceiver(dlqPath);
        return await receiver.PeekMessagesAsync(maxMessages);
    }

    // Retry: receive from DLQ then re-send to original queue
    public async Task RetryDeadLetterAsync(string queueName, long sequenceNumber)
    {
        var dlqPath = $"{queueName}/$deadletterqueue";
        var receiver = _client.CreateReceiver(dlqPath);
        var message = await receiver.ReceiveMessageAsync();
        // Complete from DLQ, re-send to original queue
        var sender = _client.CreateSender(queueName);
        await sender.SendMessageAsync(new ServiceBusMessage(message));
        await receiver.CompleteMessageAsync(message);
    }
}
```

### Pattern 5: Structured Logging with Correlation
**What:** Include CorrelationId and MessageId in all consumer log entries
**When to use:** Every consumer -- enables end-to-end tracing in Aspire dashboard
**Example:**
```csharp
public async Task Consume(ConsumeContext<ProductCreatedDomainEvent> context)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = context.CorrelationId?.ToString() ?? "none",
        ["MessageId"] = context.MessageId?.ToString() ?? "none",
        ["ConversationId"] = context.ConversationId?.ToString() ?? "none"
    });

    _logger.LogInformation("Processing ProductCreated for {ProductId}", context.Message.ProductId);
    // ... consumer logic
}
```

### Anti-Patterns to Avoid
- **Retry middleware after outbox:** Order matters. Place `UseMessageRetry` BEFORE `UseEntityFrameworkOutbox` in the pipeline. The outbox must wrap the consumer, retries must wrap the outbox.
- **Retrying permanent errors:** Never retry `ValidationException`, `ArgumentException`, or business rule violations. They will never succeed.
- **Manual idempotency checks when inbox is configured:** The EF Core inbox already tracks `MessageId` for deduplication. Consumer-level idempotency checks (like the existing `AnyAsync` in `ProductCreatedConsumer`) become redundant but harmless.
- **Mixing `UseBusOutbox` scope with consumer `UseEntityFrameworkOutbox`:** The project already uses `UseBusOutbox()` for the publish side. On the consumer side, `UseEntityFrameworkOutbox<OutboxDbContext>(context)` enables inbox deduplication. These are complementary, not conflicting.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Message deduplication | Custom `ProcessedMessages` table | MassTransit EF Core inbox (`InboxState` entity) | Already configured via `AddInboxStateEntity()`, handles cleanup automatically |
| Retry with backoff | Custom retry loop in consumer | `UseMessageRetry(r => r.Exponential(...))` | Handles exception filtering, lock extension, pipeline integration |
| Circuit breaker | Custom failure counter | `UseCircuitBreaker(cb => ...)` | Tracks failure rates, auto-resets, integrates with pipeline |
| Distributed tracing | Custom correlation middleware | MassTransit OpenTelemetry + Aspire OTLP | Auto-propagates trace context across message boundaries |
| DLQ routing | Custom error table/queue | Azure Service Bus native DLQ via `ConfigureDeadLetterQueueErrorTransport()` | Built into the broker, accessible via Azure SDK |
| Correlation ID propagation | Custom middleware to inject/read headers | MassTransit `CorrelationId` + OpenTelemetry Activity | MassTransit auto-generates CorrelationId; OpenTelemetry propagates TraceId |

**Key insight:** MassTransit 9.x provides all the messaging infrastructure patterns out of the box. The inbox is already modeled in `OutboxDbContext` via `AddInboxStateEntity()`. The phase is about activating and configuring these built-in features, not building custom solutions.

## Common Pitfalls

### Pitfall 1: Middleware Ordering in Pipeline
**What goes wrong:** Retry fires but the outbox doesn't deduplicate, or the outbox wraps retries incorrectly causing messages to be published multiple times during retries.
**Why it happens:** MassTransit middleware is a pipeline; order matters. Filters added first are outermost.
**How to avoid:** The correct order in `AddConfigureEndpointsCallback` is: `UseCircuitBreaker` -> `UseMessageRetry` -> `UseEntityFrameworkOutbox`. The circuit breaker wraps everything, retries wrap the outbox, the outbox wraps the consumer.
**Warning signs:** Duplicate messages published during retries, or retries not happening.

### Pitfall 2: Azure Service Bus Emulator DLQ Limitations
**What goes wrong:** DLQ features may behave differently in the Azure Service Bus emulator vs production.
**Why it happens:** The emulator is a local development approximation, not a full replica.
**How to avoid:** Test DLQ behavior explicitly. If the emulator does not support native DLQ sub-queues, fall back to MassTransit's `_error` queue pattern for local development.
**Warning signs:** `ConfigureDeadLetterQueueErrorTransport()` silently fails or messages disappear.

### Pitfall 3: DuplicateDetectionWindow Too Short
**What goes wrong:** Messages that arrive after the detection window are processed again (duplicates).
**Why it happens:** Default window may be shorter than retry/redelivery intervals.
**How to avoid:** Set `DuplicateDetectionWindow` to at least the sum of all retry delays plus a margin. For 3 retries at 1s/5s/25s, use at least 5 minutes.
**Warning signs:** `InboxState` entries cleaned up before redelivered messages arrive.

### Pitfall 4: Consumer Outbox Requires Same DbContext
**What goes wrong:** Inbox deduplication doesn't work because the consumer uses a different DbContext than the outbox.
**Why it happens:** `UseEntityFrameworkOutbox<OutboxDbContext>` must match the DbContext used for `AddEntityFrameworkOutbox<OutboxDbContext>`.
**How to avoid:** Always use `OutboxDbContext` as the type parameter. The project already has this centralized.
**Warning signs:** No `InboxState` records appearing in the database after consuming messages.

### Pitfall 5: Forgetting to Add MassTransit ActivitySource for Aspire Tracing
**What goes wrong:** MassTransit operations don't appear in Aspire distributed traces view.
**Why it happens:** The service defaults configure OpenTelemetry but don't add MassTransit's `ActivitySource`.
**How to avoid:** Add `tracing.AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)` to the tracing configuration in `Extensions.cs`.
**Warning signs:** HTTP requests visible in Aspire traces but message publish/consume spans missing.

### Pitfall 6: ServiceBusClient Not Available for DLQ Admin
**What goes wrong:** Cannot create `ServiceBusClient` to access DLQ programmatically.
**Why it happens:** MassTransit manages its own Azure Service Bus connection internally.
**How to avoid:** Register a separate `ServiceBusClient` using the same connection string for admin DLQ operations.
**Warning signs:** `ServiceBusClient` not resolvable from DI.

## Code Examples

### Global MassTransit Configuration (Complete)
```csharp
// Source: MassTransit official docs - middleware/outbox, exceptions, observability
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.AddEntityFrameworkOutbox<OutboxDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
    });

    // Global middleware for all consumer endpoints
    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        // 1. Circuit breaker (outermost)
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // 2. Retry with exponential backoff
        cfg.UseMessageRetry(r =>
        {
            r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(25));
            r.Ignore<PermanentException>();
        });

        // 3. EF Core consumer outbox (inbox deduplication)
        cfg.UseEntityFrameworkOutbox<OutboxDbContext>(context);
    });

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));

        // Route errors to native Azure Service Bus DLQ
        cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter(false),
            new Action<IReceiveEndpointConfigurator>(endpoint =>
            {
                if (endpoint is IServiceBusReceiveEndpointConfigurator sb)
                {
                    sb.ConfigureDeadLetterQueueErrorTransport();
                }
            }));
    });
});
```

### Adding MassTransit to OpenTelemetry Tracing
```csharp
// Source: https://masstransit.io/documentation/configuration/observability
// In ServiceDefaults/Extensions.cs, add to WithTracing:
.WithTracing(tracing =>
{
    tracing.AddSource(builder.Environment.ApplicationName)
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName) // ADD THIS
        .AddAspNetCoreInstrumentation(/* existing config */)
        .AddHttpClientInstrumentation();
});
```

### Fault Consumer for DLQ Logging
```csharp
// Source: https://masstransit.io/documentation/concepts/exceptions
public class DomainEventFaultConsumer<T> : IConsumer<Fault<T>> where T : class
{
    private readonly ILogger<DomainEventFaultConsumer<T>> _logger;

    public DomainEventFaultConsumer(ILogger<DomainEventFaultConsumer<T>> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<T>> context)
    {
        _logger.LogWarning(
            "Message {MessageType} faulted after retries. MessageId: {MessageId}, CorrelationId: {CorrelationId}, Exceptions: {Exceptions}",
            typeof(T).Name,
            context.Message.FaultedMessageId,
            context.CorrelationId,
            string.Join("; ", context.Message.Exceptions.Select(e => e.Message)));

        return Task.CompletedTask;
    }
}
```

## Discretionary Recommendations

### Correlation ID Strategy
**Recommendation:** Use MassTransit's auto-generated `CorrelationId` (Guid) for message correlation, and rely on OpenTelemetry `TraceId` (W3C format) for end-to-end distributed tracing.

**Rationale:** MassTransit's CorrelationId is a Guid, not a string, so it cannot directly hold W3C trace IDs. MassTransit already propagates OpenTelemetry trace context across message boundaries via headers automatically. The Aspire dashboard shows traces by TraceId. Using both gives: CorrelationId for business-level message grouping, TraceId for infrastructure-level distributed tracing.

**Implementation:** No custom middleware needed. MassTransit auto-generates CorrelationId if not set. OpenTelemetry trace propagation is built-in. Just ensure the MassTransit ActivitySource is added to the tracing configuration.

### Circuit Breaker Thresholds
**Recommendation:**
- `TrackingPeriod`: 1 minute
- `TripThreshold`: 15 (15% failure rate)
- `ActiveThreshold`: 10 (need 10 messages before evaluating)
- `ResetInterval`: 5 minutes

**Rationale:** Conservative defaults. 15% is low enough to catch real problems but high enough to tolerate occasional transient errors. 10-message minimum prevents tripping on low-volume queues. 5-minute reset allows downstream services to recover.

### Exponential Backoff Intervals
**Recommendation:** `Intervals(1s, 5s, 25s)` -- 3 retries with ~5x multiplier.
**Rationale:** Matches the user's decision of "3 retries before DLQ." Total retry window of ~31 seconds is within the `DuplicateDetectionWindow` of 5 minutes, ensuring inbox deduplication covers the retry period.

### Transient vs Permanent Error Classification
**Recommendation:**
- **Transient (retry):** `DbUpdateConcurrencyException`, `TimeoutException`, `HttpRequestException`, `SocketException`, `ServiceBusException` (when `IsTransient == true`)
- **Permanent (skip to DLQ):** `PermanentException` (custom marker), `ValidationException`, `ArgumentException`, `InvalidOperationException`, `FormatException`, `KeyNotFoundException`

**Implementation:** Use `r.Ignore<T>()` for permanent exceptions in retry config. Create a `PermanentException` base class that consumers can throw for business rule violations.

### Admin DLQ Page Layout
**Recommendation:** Table view matching existing admin patterns (Products/Categories pages):
- Columns: Message Type, Error Details (truncated), Correlation ID, Timestamp, Actions
- Actions: Retry (re-send to original queue), Purge (delete from DLQ)
- Batch actions: Retry All, Purge All
- Auto-refresh every 30 seconds

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Custom `ProcessedMessages` table | MassTransit EF Core inbox (`InboxState`) | MassTransit 8.x | No custom deduplication code needed |
| `OpenTelemetry.Contrib.Instrumentation.MassTransit` NuGet | Native MassTransit OpenTelemetry via `DiagnosticHeaders.DefaultListenerName` | MassTransit 8.x | No extra package needed |
| MassTransit `_error` queue only | Azure Service Bus native DLQ routing | MassTransit 8.x+ | Better integration with Azure tooling |

**Deprecated/outdated:**
- `OpenTelemetry.Contrib.Instrumentation.MassTransit` NuGet package: No longer needed. MassTransit has native OpenTelemetry support.
- Custom inbox tables: MassTransit's `AddInboxStateEntity()` provides this out of the box.

## Open Questions

1. **Azure Service Bus Emulator DLQ Support**
   - What we know: The emulator is used for local development (`RunAsEmulator()` in AppHost)
   - What's unclear: Whether the emulator fully supports native DLQ sub-queues and the `/$deadletterqueue` path
   - Recommendation: Test during implementation. If unsupported, fall back to MassTransit's `_error` queue for local dev and configure native DLQ for production only.

2. **ServiceBusClient Registration for DLQ Admin**
   - What we know: Aspire's `AddAzureServiceBus` provides the connection for MassTransit
   - What's unclear: Whether a separate `ServiceBusClient` can be resolved from DI or needs manual registration
   - Recommendation: Try resolving `ServiceBusClient` from DI first (Aspire may register it). If not available, create one from the same connection string.

3. **ConfigureDeadLetterQueueErrorTransport with ConfigureEndpoints**
   - What we know: The method exists on `IServiceBusReceiveEndpointConfigurator`
   - What's unclear: Exact integration when using `cfg.ConfigureEndpoints(context)` (auto-configured endpoints)
   - Recommendation: Use `AddConfigureEndpointsCallback` to apply this, casting the configurator to `IServiceBusReceiveEndpointConfigurator`.

## Sources

### Primary (HIGH confidence)
- [MassTransit Exceptions/Retry](https://masstransit.io/documentation/concepts/exceptions) - Retry policies, exponential backoff, exception filtering, fault consumers
- [MassTransit Outbox Configuration](https://masstransit.io/documentation/configuration/middleware/outbox) - EF Core outbox + inbox, DuplicateDetectionWindow, consumer outbox
- [MassTransit Consumer Definitions](https://masstransit.io/documentation/configuration/consumers) - Per-consumer overrides via ConsumerDefinition
- [MassTransit Observability](https://masstransit.io/documentation/configuration/observability) - OpenTelemetry integration, DiagnosticHeaders.DefaultListenerName
- [MassTransit Azure Service Bus](https://masstransit.io/documentation/configuration/transports/azure-service-bus) - ConfigureDeadLetterQueueErrorTransport, DLQ routing
- [Azure Service Bus DLQ](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues) - DLQ sub-queue path, programmatic access, receive/resubmit

### Secondary (MEDIUM confidence)
- [MassTransit CorrelationId Discussion](https://github.com/MassTransit/MassTransit/discussions/3491) - CorrelationId is Guid, use OpenTelemetry for trace propagation
- [MassTransit Circuit Breaker](https://masstransit-v6.netlify.app/advanced/middleware/circuit-breaker) - TripThreshold, ActiveThreshold, TrackingPeriod, ResetInterval (v6 docs, API stable)
- [MassTransit Transactional Outbox Pattern](https://masstransit.io/documentation/patterns/transactional-outbox) - Inbox deduplication concept

### Tertiary (LOW confidence)
- Azure Service Bus emulator DLQ support -- not verified, flagged as open question

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, versions verified from csproj
- Architecture: HIGH - Patterns verified from official MassTransit documentation
- Pitfalls: MEDIUM - Based on documentation and community discussions; middleware ordering verified, emulator DLQ behavior unverified
- DLQ Admin: MEDIUM - Azure SDK approach is documented; integration with Aspire-managed connection needs validation

**Research date:** 2026-02-08
**Valid until:** 2026-03-08 (30 days -- MassTransit 9.x is stable)
