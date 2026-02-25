# Phase 7: Ordering Domain & Checkout - Research

**Researched:** 2026-02-09
**Domain:** Order aggregate, checkout saga, accordion UI, mock payment
**Confidence:** HIGH

## Summary

This phase adds order creation with a multi-step checkout flow, guest checkout, mock payment simulation, and stock reservation orchestration via a MassTransit saga state machine. The backend introduces an Order aggregate (with OrderItem value objects) in the Ordering module, a `CheckoutSaga` state machine that coordinates stock reservation, payment simulation, and order confirmation, plus compensation handlers for rollback. The frontend adds a single-page accordion checkout (Radix UI Accordion), an order confirmation page, and checkout React Query hooks.

The codebase already has all foundational patterns established: BaseAggregateRoot, StronglyTypedId, schema-per-module DbContext, MediatR CQRS, MassTransit consumers with outbox, cookie-based BuyerIdentity, and Radix UI component primitives. The Ordering module already has a placeholder `OrderingDbContext` with the `ordering` schema configured. The Inventory module already supports `Reserve()` and `ReleaseReservation()` on StockItem.

**Primary recommendation:** Use MassTransit's `MassTransitStateMachine<CheckoutState>` with EF Core persistence (in OrderingDbContext) to orchestrate the checkout flow: reserve stock, simulate payment, create order, clear cart. Compensation on failure releases reservations. The frontend uses Radix UI Accordion for the single-page checkout layout.

## Standard Stack

### Core (Already in Project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| MassTransit | 9.0.0 | Saga state machine for checkout orchestration | Already used for domain events and outbox |
| MassTransit.EntityFrameworkCore | 9.0.0 | Saga persistence with EF Core | Saga state stored alongside ordering data |
| MediatR | 13.1.0 | Commands/queries for order CRUD | Established CQRS pattern |
| FluentValidation | 12.1.1 | Request validation | Established validation pattern |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL for ordering schema | Schema-per-module pattern |
| @radix-ui/react-accordion | (installed) | Accordion UI for checkout sections | Already in node_modules |
| @tanstack/react-query | 5.90.x | Checkout mutations and order queries | Established data-fetching pattern |
| sonner | 2.0.x | Toast notifications for checkout feedback | Established notification pattern |

### Supporting (No New Dependencies)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| radix-ui (unified) | 1.4.3 | Accordion, Collapsible, Radio primitives | Checkout form sections |
| lucide-react | 0.563.x | Icons for checkout steps | Step indicators, success/error icons |
| next-auth | 5.0.0-beta.30 | Session detection for login gate | "Continue as Guest" vs "Sign In" |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| MassTransit saga | Manual state tracking in Order entity | Saga gives built-in compensation, timeout, retry; manual is simpler but fragile |
| Radix Accordion | Radix Collapsible for each section | Accordion has built-in single-open behavior; Collapsible requires manual state |
| SagaDbContext (MT dedicated) | Existing OrderingDbContext | Using OrderingDbContext keeps saga state in same schema; SagaDbContext is MT's approach but adds another context |

**Installation:** No new packages needed. All dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
Features/Ordering/
├── Application/
│   ├── Commands/
│   │   ├── SubmitOrder/
│   │   │   ├── SubmitOrderCommand.cs         # Input: buyerId, email, shipping, cart items
│   │   │   ├── SubmitOrderCommandHandler.cs   # Creates Order, publishes OrderSubmittedEvent
│   │   │   └── SubmitOrderCommandValidator.cs
│   │   └── SimulatePayment/
│   │       ├── SimulatePaymentCommand.cs      # Input: orderId, shouldSucceed
│   │       └── SimulatePaymentCommandHandler.cs
│   ├── Queries/
│   │   └── GetOrderById/
│   │       ├── GetOrderByIdQuery.cs
│   │       ├── GetOrderByIdQueryHandler.cs
│   │       └── OrderDto.cs
│   ├── Consumers/
│   │   └── OrderSubmittedConsumer.cs          # Triggers saga start (optional)
│   └── Saga/
│       ├── CheckoutState.cs                   # SagaStateMachineInstance
│       ├── CheckoutStateMachine.cs            # MassTransitStateMachine<CheckoutState>
│       └── CheckoutStateMap.cs                # EF Core SagaClassMap
├── Domain/
│   ├── Entities/
│   │   ├── Order.cs                           # Aggregate root
│   │   └── OrderItem.cs                       # Owned entity
│   ├── ValueObjects/
│   │   ├── OrderId.cs                         # StronglyTypedId<Guid>
│   │   ├── OrderItemId.cs                     # StronglyTypedId<Guid>
│   │   ├── OrderNumber.cs                     # MC-XXXXXX format
│   │   ├── OrderStatus.cs                     # Submitted, StockReserved, Paid, Confirmed, Failed, Cancelled
│   │   └── ShippingAddress.cs                 # Value object (name, email, street, city, state, zip)
│   └── Events/
│       ├── OrderSubmittedDomainEvent.cs
│       ├── OrderPaidDomainEvent.cs
│       └── OrderFailedDomainEvent.cs
├── Infrastructure/
│   ├── OrderingDbContext.cs                   # Already exists, add DbSets + saga
│   ├── Configurations/
│   │   ├── OrderConfiguration.cs
│   │   ├── OrderItemConfiguration.cs
│   │   └── CheckoutStateConfiguration.cs      # Saga state EF config
│   └── Migrations/
└── OrderingEndpoints.cs

# Frontend
src/
├── app/(storefront)/checkout/
│   └── page.tsx                               # Checkout page
├── app/(storefront)/order-confirmation/[id]/
│   └── page.tsx                               # Order confirmation page
├── components/storefront/
│   ├── checkout-page.tsx                      # Main checkout orchestrator
│   ├── checkout-accordion.tsx                 # Accordion wrapper with sections
│   ├── shipping-section.tsx                   # Name, email, address form
│   ├── payment-section.tsx                    # Pay Now + simulate toggle
│   ├── order-sidebar.tsx                      # Sticky order summary
│   ├── checkout-login-gate.tsx                # "Continue as Guest" or "Sign In"
│   └── order-confirmation.tsx                 # Full order summary
├── hooks/
│   └── use-checkout.ts                        # Checkout mutations + order query
└── lib/
    └── api.ts                                 # Add ordering API functions
```

### Pattern 1: Order Aggregate with Factory Method
**What:** Order aggregate root following same pattern as Product and Cart
**When to use:** Creating orders from checkout submission
**Example:**
```csharp
// Follows existing BaseAggregateRoot<TId> pattern (source: codebase)
public sealed class Order : BaseAggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];

    public OrderNumber OrderNumber { get; private set; } = null!;
    public Guid BuyerId { get; private set; }          // Same as Cart.BuyerId
    public string BuyerEmail { get; private set; } = null!;
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    [Timestamp]
    public uint Version { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order(OrderId id) : base(id) { }

    public static Order Create(
        Guid buyerId, string buyerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items)
    {
        var order = new Order(OrderId.New())
        {
            OrderNumber = OrderNumber.Generate(),
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Submitted,
            ShippingCost = 5.99m,   // Flat rate mock
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Create(
                order.Id, item.productId, item.productName,
                item.unitPrice, item.imageUrl, item.quantity));
        }

        order.Subtotal = order._items.Sum(i => i.LineTotal);
        order.Tax = Math.Round(order.Subtotal * 0.08m, 2);  // 8% mock tax
        order.Total = order.Subtotal + order.ShippingCost + order.Tax;

        order.AddDomainEvent(new OrderSubmittedDomainEvent(order.Id.Value));
        return order;
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new OrderPaidDomainEvent(Id.Value));
    }

    public void MarkAsFailed(string reason)
    {
        Status = OrderStatus.Failed;
        FailureReason = reason;
        AddDomainEvent(new OrderFailedDomainEvent(Id.Value));
    }

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
    }
}
```

### Pattern 2: Checkout Saga State Machine
**What:** MassTransit state machine orchestrating checkout steps
**When to use:** Coordinating stock reservation, payment, order creation across modules
**Example:**
```csharp
// Source: https://masstransit.io/documentation/patterns/saga/state-machine
public class CheckoutState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }        // = OrderId
    public string CurrentState { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public string? BuyerEmail { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public string? FailureReason { get; set; }

    // Track reservations for compensation
    public string? ReservationIdsJson { get; set; }  // serialized Dict<ProductId, ReservationId>

    // PostgreSQL xmin concurrency
    public uint RowVersion { get; set; }
}

public class CheckoutStateMachine : MassTransitStateMachine<CheckoutState>
{
    // States
    public State Submitted { get; private set; } = null!;
    public State StockReserved { get; private set; } = null!;
    public State PaymentProcessing { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
    public State Confirmed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // Events (message contracts)
    public Event<CheckoutStarted> CheckoutStarted { get; private set; } = null!;
    public Event<StockReservationCompleted> StockReserved { get; private set; } = null!;
    public Event<StockReservationFailed> StockReservationFailed { get; private set; } = null!;
    public Event<PaymentCompleted> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;

    public CheckoutStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CheckoutStarted, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReserved, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentCompleted, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(CheckoutStarted)
                .Then(ctx => {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.BuyerId = ctx.Message.BuyerId;
                    ctx.Saga.BuyerEmail = ctx.Message.BuyerEmail;
                    ctx.Saga.SubmittedAt = DateTimeOffset.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<ReserveStockForOrder>(new {
                    ctx.Message.OrderId,
                    ctx.Message.Items
                }))
                .TransitionTo(Submitted));

        During(Submitted,
            When(StockReserved)
                .Then(ctx => ctx.Saga.ReservationIdsJson = ctx.Message.ReservationIdsJson)
                .TransitionTo(StockReserved),
                // Payment triggered by frontend after stock reserved
            When(StockReservationFailed)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .PublishAsync(ctx => ctx.Init<OrderFailed>(new {
                    ctx.Saga.OrderId,
                    Reason = ctx.Message.Reason
                }))
                .TransitionTo(Failed)
                .Finalize());

        During(StockReserved,
            When(PaymentCompleted)
                .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new { ctx.Saga.OrderId }))
                .PublishAsync(ctx => ctx.Init<DeductStock>(new {
                    ctx.Saga.OrderId,
                    ctx.Saga.ReservationIdsJson
                }))
                .TransitionTo(Confirmed)
                .Finalize(),
            When(PaymentFailed)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                // Compensation: release all reservations
                .PublishAsync(ctx => ctx.Init<ReleaseStockReservations>(new {
                    ctx.Saga.OrderId,
                    ctx.Saga.ReservationIdsJson
                }))
                .PublishAsync(ctx => ctx.Init<OrderFailed>(new {
                    ctx.Saga.OrderId,
                    Reason = ctx.Message.Reason
                }))
                .TransitionTo(Failed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
```

### Pattern 3: Saga Registration with EF Core
**What:** Register saga state machine with EF Core persistence in OrderingDbContext
**When to use:** Configuring DI in Program.cs
**Example:**
```csharp
// Source: https://masstransit.io/documentation/configuration/persistence/entity-framework
// In Program.cs, add to existing MassTransit configuration:
x.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
        r.ExistingDbContext<OrderingDbContext>();
        r.UsePostgres();
    });

// In OrderingDbContext.OnModelCreating, add saga entity configuration
// or use IEntityTypeConfiguration<CheckoutState> in Configurations folder
```

### Pattern 4: Order Number Generation
**What:** Random alphanumeric order number with MC- prefix
**When to use:** Creating new orders
**Example:**
```csharp
public sealed record OrderNumber
{
    private static readonly char[] Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    public string Value { get; }
    private OrderNumber(string value) => Value = value;

    public static OrderNumber Generate()
    {
        var chars = new char[6];
        for (var i = 0; i < 6; i++)
            chars[i] = Chars[Random.Shared.Next(Chars.Length)];
        return new OrderNumber($"MC-{new string(chars)}");
    }
}
```

### Anti-Patterns to Avoid
- **Calling inventory directly from order handler:** Use saga events, not direct cross-module calls. Modules communicate only via messages.
- **Storing full cart snapshot in order:** Copy only needed fields (productId, name, price, qty, imageUrl) as OrderItem value objects. Don't reference cart entities.
- **Blocking on payment simulation:** The mock payment should be async via saga event, not a synchronous call in the order handler.
- **Clearing cart before order confirmed:** Cart should only be cleared after successful payment + order confirmation, not on submit.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Saga orchestration | Manual state tracking with if/else | MassTransit StateMachine | Built-in compensation, timeout, retry, persistence, concurrency |
| Checkout state persistence | Custom state table + manual SQL | MassTransit EF Core saga repository | Automatic concurrency, state serialization, cleanup |
| Accordion behavior | Custom useState toggle logic | Radix UI Accordion (type="single") | Accessible, animated, keyboard-navigable out of box |
| Order number uniqueness | UUID or auto-increment | Random alphanumeric with collision check | User-friendly, memorable, no sequential guessing |
| Optimistic concurrency | Manual version checking | PostgreSQL xmin + EF Core [Timestamp] | Already established pattern in Cart and StockItem |

**Key insight:** The checkout flow is a distributed transaction across 3 modules (Cart, Inventory, Ordering). A saga makes the coordination explicit and compensation automatic. Hand-rolling this with try/catch leads to inconsistent state on partial failures.

## Common Pitfalls

### Pitfall 1: Saga CorrelationId Mismatch
**What goes wrong:** Events don't reach the saga instance because CorrelationId doesn't match.
**Why it happens:** Each event message needs an OrderId property that matches the saga's CorrelationId. If the property name differs or the value is wrong, the saga instance is never found.
**How to avoid:** All saga event contracts must have an `OrderId` property of type `Guid`. Use `x.CorrelateById(ctx => ctx.Message.OrderId)` consistently.
**Warning signs:** Saga stays in Initial state; events go to error queue.

### Pitfall 2: Saga Concurrency with Optimistic Locking
**What goes wrong:** Two events arrive simultaneously for the same saga, one fails with concurrency exception.
**Why it happens:** PostgreSQL xmin changes on every update. Two concurrent saga transitions conflict.
**How to avoid:** MassTransit retries automatically on concurrency conflicts when using `ConcurrencyMode.Optimistic`. Ensure retry middleware is configured (already done in Program.cs).
**Warning signs:** Intermittent `DbUpdateConcurrencyException` in saga processing.

### Pitfall 3: Cart Clearing Before Confirmation
**What goes wrong:** User's cart is emptied but order fails, leaving user with no cart and no order.
**Why it happens:** Cart cleared optimistically before payment succeeds.
**How to avoid:** Clear cart only AFTER order is confirmed (saga reaches Confirmed state). Publish a `ClearCart` event from the saga on successful completion.
**Warning signs:** Users report empty carts after checkout errors.

### Pitfall 4: Missing Compensation on Stock Reservation
**What goes wrong:** Payment fails but stock reservations are never released, eventually expiring via TTL.
**Why it happens:** Compensation handler not wired up, or saga doesn't publish release events on failure.
**How to avoid:** Every failure path in the saga MUST publish `ReleaseStockReservations`. The saga tracks reservation IDs specifically for this purpose.
**Warning signs:** Available stock drops after failed checkouts (until 15-min TTL expires).

### Pitfall 5: Guest Checkout Email Not Persisted
**What goes wrong:** Guest completes checkout but can never look up their order.
**Why it happens:** Email captured in form but not stored on the Order aggregate.
**How to avoid:** `Order.BuyerEmail` is a required field. For guest checkout, email comes from the shipping form. For authenticated users, email comes from the token/profile.
**Warning signs:** Orders in database with no way to identify the buyer.

## Code Examples

### Checkout API Endpoint
```csharp
// Source: follows CartEndpoints.cs pattern
group.MapPost("/checkout", async (
    CheckoutRequest request,
    HttpContext httpContext,
    ISender sender,
    CancellationToken ct) =>
{
    var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);
    var command = new SubmitOrderCommand(
        buyerId, request.Email, request.ShippingAddress, request.Items);
    var result = await sender.Send(command, ct);
    return Results.Created($"/api/orders/{result.OrderId}", result);
});
```

### Checkout Accordion (Frontend)
```tsx
// Source: Radix UI Accordion + shadcn pattern
import * as Accordion from "@radix-ui/react-accordion";

function CheckoutAccordion({ activeSection, onSectionComplete }) {
  return (
    <Accordion.Root type="single" value={activeSection}>
      <Accordion.Item value="shipping">
        <Accordion.Trigger>Shipping Information</Accordion.Trigger>
        <Accordion.Content>
          <ShippingSection onComplete={() => onSectionComplete("payment")} />
        </Accordion.Content>
      </Accordion.Item>
      <Accordion.Item value="payment">
        <Accordion.Trigger>Payment</Accordion.Trigger>
        <Accordion.Content>
          <PaymentSection />
        </Accordion.Content>
      </Accordion.Item>
    </Accordion.Root>
  );
}
```

### Checkout Hook Pattern
```typescript
// Source: follows use-cart.ts pattern with optimistic mutations
export function useSubmitOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SubmitOrderRequest) => submitOrder(data),
    onSuccess: (result) => {
      // Clear cart cache after successful order
      queryClient.setQueryData(["cart"], null);
      // Redirect handled by caller
    },
    onError: () => {
      toast.error("Failed to submit order. Please try again.");
    },
  });
}
```

### Saga EF Core Configuration
```csharp
// Source: https://masstransit.io/documentation/configuration/persistence/entity-framework
public sealed class CheckoutStateConfiguration : IEntityTypeConfiguration<CheckoutState>
{
    public void Configure(EntityTypeBuilder<CheckoutState> builder)
    {
        builder.ToTable("CheckoutSagas");
        builder.HasKey(x => x.CorrelationId);
        builder.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OrderId);
        builder.Property(x => x.BuyerId);
        builder.Property(x => x.BuyerEmail).HasMaxLength(256);
        builder.Property(x => x.SubmittedAt);
        builder.Property(x => x.FailureReason).HasMaxLength(1024);
        builder.Property(x => x.ReservationIdsJson).HasMaxLength(4096);

        // PostgreSQL xmin optimistic concurrency
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Automatonymous (separate package) | MassTransitStateMachine (built-in) | MassTransit v8 | Saga state machines are first-class in MT; no separate NuGet |
| Pessimistic DB locks for saga | Optimistic concurrency with retry | MT v8+ | Better throughput, no lock contention |
| SagaDbContext (dedicated) | ExistingDbContext<T> option | MT v8+ | Can reuse module's DbContext instead of creating separate one |
| Manual saga class map | IEntityTypeConfiguration | EF Core pattern | Standard EF Core config pattern, not MT-specific SagaClassMap |

**Deprecated/outdated:**
- `Automatonymous` NuGet package: Merged into MassTransit core as of v8. Do not install separately.
- `SagaDbContext` base class: Still works but `ExistingDbContext<T>` is simpler when you already have a module DbContext.

## Open Questions

1. **Saga vs direct orchestration for this simple flow**
   - What we know: The checkout has only 3 steps (reserve stock, pay, confirm). A full saga with message bus roundtrips adds latency.
   - What's unclear: Whether the added complexity of a saga is justified for a demo project with a monolith.
   - Recommendation: Use the saga. It demonstrates the pattern (core project goal), handles compensation correctly, and aligns with the deliverables spec. The latency is acceptable for a demo. If simplicity is preferred, a simpler "process manager" pattern (just a service class calling handlers in sequence with try/catch compensation) could work, but loses the persistence and retry benefits.

2. **Where to store CheckoutState: OrderingDbContext vs separate SagaDbContext**
   - What we know: MassTransit supports `ExistingDbContext<OrderingDbContext>` to share the same context. SagaDbContext is the "official" approach but adds another context.
   - What's unclear: Whether EF Core migration generation works smoothly with saga entities in a shared context.
   - Recommendation: Use `ExistingDbContext<OrderingDbContext>`. Keeps the ordering schema self-contained. Add `IEntityTypeConfiguration<CheckoutState>` alongside other ordering configs. This is the simpler approach and matches the existing pattern.

3. **Stock deduction vs reservation release timing**
   - What we know: On successful payment, stock must be permanently deducted (not just reserved). Reservations should be released/converted.
   - What's unclear: Whether to deduct stock (reduce QuantityOnHand) and then remove the reservation, or just remove the reservation and treat the original reserve as the deduction.
   - Recommendation: On payment success, deduct stock (AdjustStock with negative quantity) AND release the reservation. This makes the permanent deduction explicit and the available quantity accurate immediately.

## Sources

### Primary (HIGH confidence)
- [MassTransit State Machine docs](https://masstransit.io/documentation/patterns/saga/state-machine) - State machine structure, events, transitions, compensation
- [MassTransit EF Core persistence](https://masstransit.io/documentation/configuration/persistence/entity-framework) - Saga repository config, PostgreSQL xmin, SagaClassMap
- [MassTransit Saga Overview](https://masstransit.io/documentation/patterns/saga) - Saga patterns, correlation, finalization
- Codebase analysis: Cart, Inventory, Catalog modules - Established patterns for aggregates, CQRS, consumers, configurations
- [Radix UI Accordion](https://www.radix-ui.com/primitives/docs/components/accordion) - Accordion primitives API
- [shadcn/ui Accordion](https://ui.shadcn.com/docs/components/radix/accordion) - Styled accordion component

### Secondary (MEDIUM confidence)
- [Milan Jovanovic - Saga Pattern with MassTransit](https://www.milanjovanovic.tech/blog/implementing-the-saga-pattern-with-masstransit) - Practical saga implementation patterns
- [DEV Community - Order Processing Saga](https://dev.to/stevsharp/building-an-order-processing-saga-with-masstransit-i9j) - Order saga with compensation

### Tertiary (LOW confidence)
- None - all findings verified against official docs or codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, no new dependencies
- Architecture: HIGH - Saga pattern verified against MassTransit official docs, aggregate pattern matches existing codebase
- Pitfalls: HIGH - Compensation and correlation patterns documented in official MassTransit docs
- Frontend: HIGH - Radix UI Accordion already installed, follows existing component patterns

**Research date:** 2026-02-09
**Valid until:** 2026-03-11 (30 days - stable stack, no fast-moving dependencies)
