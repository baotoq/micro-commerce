# Architecture Research

**Domain:** E-commerce Microservices
**Researched:** 2026-01-29
**Confidence:** HIGH (verified with Microsoft official documentation and eShop reference architecture)

## Standard Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CLIENT LAYER                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    Next.js Frontend (BFF)                            │    │
│  │              NextAuth.js ←→ Keycloak (Identity)                      │    │
│  └────────────────────────────┬────────────────────────────────────────┘    │
├───────────────────────────────┼─────────────────────────────────────────────┤
│                               ↓                                              │
│                        API GATEWAY                                           │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │    YARP / Aspire Gateway (routes, aggregates, auth validation)       │    │
│  └────────────────────────────┬────────────────────────────────────────┘    │
├───────────────────────────────┼─────────────────────────────────────────────┤
│                               ↓                                              │
│                        SERVICE LAYER                                         │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐            │
│  │  Catalog   │  │    Cart    │  │  Ordering  │  │ Inventory  │            │
│  │  Service   │  │  Service   │  │  Service   │  │  Service   │            │
│  └─────┬──────┘  └─────┬──────┘  └─────┬──────┘  └─────┬──────┘            │
│        │               │               │               │                    │
├────────┼───────────────┼───────────────┼───────────────┼────────────────────┤
│                     EVENT BUS (Azure Service Bus)                            │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  ProductCreated  │  CartCheckedOut  │  OrderPlaced  │  StockReserved │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
├───────────────────────────────┼─────────────────────────────────────────────┤
│                        DATA LAYER (database-per-service)                     │
│  ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐           │
│  │ Catalog  │     │  Cart    │     │ Ordering │     │Inventory │           │
│  │ Postgres │     │ Postgres │     │ Postgres │     │ Postgres │           │
│  └──────────┘     └──────────┘     └──────────┘     └──────────┘           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|------------------------|
| **Catalog Service** | Product management, search, categories | REST API, read-heavy, cacheable |
| **Cart Service** | Shopping cart state, guest carts, cart expiration | REST API, session-based, guest-friendly |
| **Ordering Service** | Order lifecycle, checkout, order history | REST API, CQRS/DDD patterns, saga orchestration |
| **Inventory Service** | Stock levels, reservations, availability | Event-driven, consistency-critical |
| **API Gateway** | Request routing, auth validation, aggregation | YARP or custom Aspire routing |
| **Event Bus** | Async service communication, decoupling | Azure Service Bus with topics/subscriptions |

## Service Boundaries (Bounded Contexts)

### Catalog Service

**Domain:** Product information, categories, search

**Owns:**
- Product aggregate (id, name, description, price, images)
- Category aggregate (id, name, parent)
- Product-Category relationships

**API Endpoints:**
```
GET  /api/catalog/products         # List/search products
GET  /api/catalog/products/{id}    # Product details
GET  /api/catalog/categories       # List categories
POST /api/catalog/products         # Admin: create product
PUT  /api/catalog/products/{id}    # Admin: update product
```

**Events Published:**
- `ProductCreated` - New product available
- `ProductUpdated` - Product details changed (especially price)
- `ProductDeleted` - Product discontinued

**Events Subscribed:**
- None (upstream service, minimal dependencies)

**Data Characteristics:**
- Read-heavy (cache-friendly)
- Eventual consistency acceptable
- Full-text search capability needed

---

### Cart Service

**Domain:** Shopping cart management

**Owns:**
- Cart aggregate (id, buyerId/guestId, items, created, lastModified)
- CartItem value object (productId, productName, price, quantity)

**API Endpoints:**
```
GET    /api/cart                   # Get current cart
POST   /api/cart/items             # Add item to cart
PUT    /api/cart/items/{productId} # Update quantity
DELETE /api/cart/items/{productId} # Remove item
DELETE /api/cart                   # Clear cart
```

**Events Published:**
- `CartCheckedOut` - Cart converted to order attempt

**Events Subscribed:**
- `ProductUpdated` - Update cached product info in cart items
- `ProductDeleted` - Remove unavailable products from carts

**Data Characteristics:**
- Short-lived data (cart expiration)
- Guest cart support (cookie-based identification)
- Denormalized product info (cart stores snapshot at add-time)

---

### Ordering Service

**Domain:** Order lifecycle management

**Owns:**
- Order aggregate (id, buyerId, items, status, totals, timestamps)
- OrderItem value object (productId, productName, price, quantity)
- OrderStatus (Submitted, AwaitingValidation, StockConfirmed, Paid, Shipped, Cancelled)

**API Endpoints:**
```
POST /api/orders                   # Create order (checkout)
GET  /api/orders                   # List user's orders
GET  /api/orders/{id}              # Order details
PUT  /api/orders/{id}/cancel       # Cancel order
```

**Events Published:**
- `OrderSubmitted` - New order, needs stock validation
- `OrderStockConfirmed` - Stock reserved, ready for payment
- `OrderPaid` - Payment successful
- `OrderCancelled` - Order cancelled (triggers stock release)

**Events Subscribed:**
- `StockConfirmed` - Inventory confirmed stock available
- `StockRejected` - Insufficient stock
- `PaymentSucceeded` - External payment confirmation
- `PaymentFailed` - Payment declined

**Data Characteristics:**
- Write-heavy during checkout
- Order history immutable
- Saga orchestrator for order flow

---

### Inventory Service

**Domain:** Stock management

**Owns:**
- StockItem aggregate (productId, availableQuantity, reservedQuantity)
- StockReservation value object (orderId, quantity, expiresAt)

**API Endpoints:**
```
GET  /api/inventory/{productId}    # Stock availability
POST /api/inventory/reserve        # Reserve stock (internal)
POST /api/inventory/release        # Release reservation (internal)
```

**Events Published:**
- `StockConfirmed` - Stock reserved for order
- `StockRejected` - Insufficient stock
- `StockReleased` - Reservation cancelled/expired
- `LowStockWarning` - Stock below threshold

**Events Subscribed:**
- `OrderSubmitted` - Trigger stock reservation
- `OrderCancelled` - Release reserved stock
- `OrderPaid` - Confirm stock deduction
- `ProductCreated` - Initialize stock record

**Data Characteristics:**
- Consistency-critical (prevent overselling)
- Reservation timeout handling
- May need optimistic concurrency

## Event Flows

### Checkout Flow (Happy Path)

```
┌─────────┐     ┌──────────┐     ┌───────────┐     ┌───────────┐
│  Cart   │     │ Ordering │     │ Inventory │     │  Payment  │
└────┬────┘     └────┬─────┘     └─────┬─────┘     └─────┬─────┘
     │               │                 │                 │
     │ CartCheckedOut│                 │                 │
     │──────────────>│                 │                 │
     │               │                 │                 │
     │               │ OrderSubmitted  │                 │
     │               │────────────────>│                 │
     │               │                 │                 │
     │               │                 │ Reserve Stock   │
     │               │                 │─────────────────│
     │               │                 │                 │
     │               │ StockConfirmed  │                 │
     │               │<────────────────│                 │
     │               │                 │                 │
     │               │ OrderStockConfirmed               │
     │               │──────────────────────────────────>│
     │               │                 │                 │
     │               │                 │  PaymentSuccess │
     │               │<──────────────────────────────────│
     │               │                 │                 │
     │               │ OrderPaid       │                 │
     │               │────────────────>│                 │
     │               │                 │                 │
     │               │                 │ Deduct Stock    │
     │               │                 │─────────────────│
```

### Checkout Flow (Stock Rejected)

```
┌─────────┐     ┌──────────┐     ┌───────────┐
│  Cart   │     │ Ordering │     │ Inventory │
└────┬────┘     └────┬─────┘     └─────┬─────┘
     │               │                 │
     │ CartCheckedOut│                 │
     │──────────────>│                 │
     │               │                 │
     │               │ OrderSubmitted  │
     │               │────────────────>│
     │               │                 │
     │               │ StockRejected   │
     │               │<────────────────│
     │               │                 │
     │               │ OrderCancelled  │
     │               │ (insufficient)  │
     │<──────────────│                 │
```

### Product Price Changed

```
┌──────────┐     ┌──────────┐     ┌───────────┐
│ Catalog  │     │   Cart   │     │  Ordering │
└────┬─────┘     └────┬─────┘     └─────┬─────┘
     │                │                 │
     │ ProductUpdated │                 │
     │───────────────>│                 │
     │                │                 │
     │                │ Update cached   │
     │                │ prices in carts │
     │                │                 │
     │ ProductUpdated │                 │
     │───────────────────────────────>  │
     │                │                 │
     │                │  (No action -   │
     │                │   orders have   │
     │                │   locked prices)│
```

## Data Ownership and Eventual Consistency

### Data Ownership Matrix

| Data | Owner Service | Other Services |
|------|---------------|----------------|
| Product details | Catalog | Cart (cached snapshot), Ordering (frozen at order time) |
| Product price | Catalog | Cart (cached, refreshable), Ordering (locked at order) |
| Stock levels | Inventory | Catalog (cached for display) |
| Cart contents | Cart | Ordering (copied at checkout) |
| Order history | Ordering | Cart (references), Inventory (reservations) |
| Reservations | Inventory | Ordering (references) |

### Eventual Consistency Strategies

**1. Data Denormalization**
- Cart stores product name + price at add-time
- Order stores frozen product info (name, price, image URL)
- Prevents coupling to Catalog during order display

**2. Saga Pattern for Distributed Transactions**
- Checkout uses choreography-based saga
- Each step publishes event, next service reacts
- Compensating actions for rollback (release stock, cancel order)

**3. Optimistic UI**
- Frontend shows immediate feedback
- Background sync corrects eventual inconsistencies
- Stock availability is "best effort" until checkout

**4. Idempotent Event Handlers**
- Events may be delivered multiple times
- Use event ID deduplication
- Handlers check "already processed" before acting

### Consistency Boundaries

| Operation | Consistency Level | Why |
|-----------|------------------|-----|
| View product | Eventual | Stale data acceptable |
| View stock | Eventual | "In stock" is approximate |
| Add to cart | Immediate (within Cart) | User expects instant feedback |
| Checkout | Eventual across services | Saga coordinates multi-service |
| Reserve stock | Strong (within Inventory) | Prevent overselling |
| Order status | Eventual | Status updates async |

## Recommended Project Structure

### Monolith Phase (Current → Phase 1-2)

```
code/
├── MicroCommerce.AppHost/            # Aspire orchestration
│   └── AppHost.cs
├── MicroCommerce.ServiceDefaults/    # Shared service configuration
├── MicroCommerce.ApiService/         # Single API (all domains)
│   ├── Program.cs
│   ├── Features/
│   │   ├── Catalog/
│   │   │   ├── Domain/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Category.cs
│   │   │   │   └── Events/
│   │   │   ├── Application/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── Infrastructure/
│   │   │   │   └── CatalogDbContext.cs
│   │   │   └── Endpoints/
│   │   │       └── CatalogEndpoints.cs
│   │   ├── Cart/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   └── Endpoints/
│   │   ├── Ordering/
│   │   │   └── ... (same structure)
│   │   └── Inventory/
│   │       └── ... (same structure)
│   └── Shared/
│       ├── IntegrationEvents/        # Events shared between modules
│       └── EventBus/                 # In-memory for monolith
├── BuildingBlocks/
│   └── BuildingBlocks.Common/        # DDD building blocks
└── MicroCommerce.Web/                # Next.js frontend
```

### Microservices Phase (Phase 3+)

```
code/
├── MicroCommerce.AppHost/            # Aspire orchestration (all services)
├── MicroCommerce.ServiceDefaults/    # Shared across all services
├── Services/
│   ├── Catalog/
│   │   ├── Catalog.Api/              # Extracted service
│   │   │   ├── Program.cs
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   └── Endpoints/
│   │   └── Catalog.IntegrationEvents/
│   ├── Cart/
│   │   ├── Cart.Api/
│   │   └── Cart.IntegrationEvents/
│   ├── Ordering/
│   │   ├── Ordering.Api/
│   │   └── Ordering.IntegrationEvents/
│   └── Inventory/
│       ├── Inventory.Api/
│       └── Inventory.IntegrationEvents/
├── ApiGateway/
│   └── MicroCommerce.Gateway/        # YARP-based gateway
├── BuildingBlocks/
│   └── BuildingBlocks.Common/
└── MicroCommerce.Web/
```

### Structure Rationale

- **Features/ folder in monolith:** Vertical slice by domain, easy to extract later
- **Domain/Application/Infrastructure:** Clean Architecture within each feature
- **IntegrationEvents shared:** Contracts between services (NuGet packages in microservices phase)
- **ApiGateway separate:** Dedicated routing, doesn't contain business logic

## Architectural Patterns

### Pattern 1: Vertical Slice Architecture

**What:** Organize code by feature/domain rather than technical layer
**When to use:** Always - supports gradual extraction
**Trade-offs:**
- (+) Easy service extraction (slice becomes service)
- (+) Changes isolated to single slice
- (-) Some duplication of infrastructure patterns

**Example:**
```csharp
// Features/Catalog/Endpoints/CatalogEndpoints.cs
public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog");

        group.MapGet("/products", GetProducts);
        group.MapGet("/products/{id}", GetProduct);
        group.MapPost("/products", CreateProduct).RequireAuthorization("admin");
    }
}
```

### Pattern 2: Outbox Pattern for Reliable Messaging

**What:** Store events in database, then publish asynchronously
**When to use:** When events must not be lost (order created, stock reserved)
**Trade-offs:**
- (+) Guaranteed delivery
- (+) Atomic with business transaction
- (-) Adds complexity (outbox processor)

**Example:**
```csharp
// Within transaction
await dbContext.Orders.AddAsync(order);
await dbContext.OutboxMessages.AddAsync(new OutboxMessage
{
    EventType = nameof(OrderSubmitted),
    Payload = JsonSerializer.Serialize(orderSubmittedEvent),
    CreatedAt = DateTime.UtcNow
});
await dbContext.SaveChangesAsync(); // Atomic commit

// Separate background process publishes and marks processed
```

### Pattern 3: Saga with Choreography

**What:** Each service listens to events and decides next action
**When to use:** Simple workflows (checkout with 3-4 services)
**Trade-offs:**
- (+) No central orchestrator (no single point of failure)
- (+) Services remain independent
- (-) Hard to visualize full workflow
- (-) Debugging distributed flow is complex

**Example Choreography:**
```csharp
// Ordering Service - handles CartCheckedOut
public class CartCheckedOutHandler : IIntegrationEventHandler<CartCheckedOut>
{
    public async Task Handle(CartCheckedOut @event)
    {
        var order = Order.Create(@event.BuyerId, @event.Items);
        await _orderRepository.AddAsync(order);
        await _eventBus.PublishAsync(new OrderSubmitted(order.Id, order.Items));
    }
}

// Inventory Service - handles OrderSubmitted
public class OrderSubmittedHandler : IIntegrationEventHandler<OrderSubmitted>
{
    public async Task Handle(OrderSubmitted @event)
    {
        var reserved = await _stockService.TryReserve(@event.OrderId, @event.Items);

        if (reserved)
            await _eventBus.PublishAsync(new StockConfirmed(@event.OrderId));
        else
            await _eventBus.PublishAsync(new StockRejected(@event.OrderId));
    }
}
```

## Data Flow

### Request Flow (API Gateway)

```
Browser Request
    ↓
Next.js BFF (handles auth, calls backend)
    ↓
API Gateway (routes to service)
    ↓
Service Endpoint (validates, processes)
    ↓
MediatR Handler (business logic)
    ↓
Domain Model (enforces invariants)
    ↓
Repository (persists)
    ↓
Database
```

### Event Flow (Service to Service)

```
Service A (publishes)
    ↓
Outbox Table (atomic with transaction)
    ↓
Outbox Processor (background)
    ↓
Azure Service Bus (topic)
    ↓
Subscription (filtered)
    ↓
Service B Handler (processes)
    ↓
Idempotency Check
    ↓
Business Logic
```

### Key Data Flows

1. **Browse Products:** Frontend → Gateway → Catalog → Cache/DB → Response (read-optimized)
2. **Add to Cart:** Frontend → Gateway → Cart → DB → Response (session-affinity)
3. **Checkout:** Cart → OrderSubmitted → Inventory → StockConfirmed → Ordering → PaymentRequest (saga)
4. **Price Change:** Catalog → ProductUpdated → Cart (update cached), Inventory (no action)

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 0-1k users | Monolith is fine - single API, single database |
| 1k-100k users | Extract Catalog (read-heavy), add Redis cache |
| 100k+ users | Full extraction, database-per-service, horizontal scaling per service |

### Scaling Priorities

1. **First bottleneck:** Catalog reads - add Redis/CDN caching, consider read replicas
2. **Second bottleneck:** Cart writes during peak - horizontal scaling, sticky sessions or distributed cache
3. **Third bottleneck:** Checkout flow - scale Ordering/Inventory independently, ensure event bus capacity

### Gradual Extraction Order

Based on dependencies and complexity:

```
Phase 1: Modular Monolith
    ↓ (establish boundaries, in-memory events)

Phase 2: Extract Catalog Service (lowest coupling, read-heavy)
    ↓ (easiest extraction, good first microservice)

Phase 3: Extract Inventory Service (event-driven, consistency-critical)
    ↓ (decouples stock from ordering)

Phase 4: Extract Cart Service (session data, guest support)
    ↓ (isolated concern)

Phase 5: Extract Ordering Service (saga orchestration, complex)
    ↓ (most complex, benefits from other services being stable)
```

## Anti-Patterns

### Anti-Pattern 1: Shared Database

**What people do:** All services read/write to same database tables
**Why it's wrong:** Tight coupling, schema changes affect all services, no true isolation
**Do this instead:** Database-per-service with data duplication via events

### Anti-Pattern 2: Synchronous Service Chains

**What people do:** Service A calls B calls C calls D synchronously
**Why it's wrong:** Latency multiplication, single point of failure, coupling
**Do this instead:** Async events for cross-service workflows, aggregate queries for reads

### Anti-Pattern 3: Distributed Monolith

**What people do:** Extract services but keep tight coupling via sync calls
**Why it's wrong:** Worst of both worlds - network latency + coupling
**Do this instead:** True boundaries, event-driven communication, data duplication acceptable

### Anti-Pattern 4: Anemic Domain Model

**What people do:** Entities with only getters/setters, logic in services
**Why it's wrong:** Business rules scattered, hard to maintain invariants
**Do this instead:** Rich domain model with behavior, aggregates enforce consistency

### Anti-Pattern 5: Premature Microservices

**What people do:** Start with microservices before understanding domain
**Why it's wrong:** Wrong boundaries, excessive ceremony, network complexity for no benefit
**Do this instead:** Start modular monolith, extract when pain points emerge

## Integration Points

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| Keycloak | OAuth 2.0 / OIDC | JWT validation at gateway, user info propagated |
| Payment (mock) | Async events | PaymentRequested → PaymentSucceeded/Failed |
| Azure Service Bus | Topic/Subscription | One topic per event type, filtered subscriptions |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Catalog ↔ Cart | Integration Events | Cart caches product info, updates on ProductUpdated |
| Cart ↔ Ordering | Integration Events | CartCheckedOut triggers order creation |
| Ordering ↔ Inventory | Integration Events | Saga: OrderSubmitted → StockConfirmed/Rejected |
| All ↔ Gateway | REST/gRPC | Gateway routes, services don't know about each other |

## Build Order Implications

### Recommended Implementation Sequence

| Order | Component | Dependencies | Rationale |
|-------|-----------|--------------|-----------|
| 1 | Catalog (domain + API) | None | Foundation, no dependencies on other services |
| 2 | Inventory (domain + API) | Catalog events | Needs product reference, enables stock display |
| 3 | Cart (domain + API) | Catalog events | Needs product info, caches product data |
| 4 | Event Bus infrastructure | All services | Connect services before ordering |
| 5 | Ordering (domain + API) | All of above | Orchestrates saga, depends on all |
| 6 | API Gateway | All services running | Routes to all services |
| 7 | Full saga implementation | All of above | End-to-end checkout flow |

### Critical Path

```
Catalog → Inventory → Cart → Ordering → Gateway → E2E Tests
```

Each service should be functional (API + domain + persistence) before starting the next, but event integration can be added incrementally.

## Sources

- [Microsoft .NET Microservices Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/) - HIGH confidence
- [eShop Reference Application](https://github.com/dotnet/eShop) - HIGH confidence
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/) - HIGH confidence
- [Saga Pattern](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga) - HIGH confidence
- [Strangler Fig Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/strangler-fig) - HIGH confidence
- [Data Sovereignty per Microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice) - HIGH confidence
- [Integration Event Communication](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications) - HIGH confidence
- [API Gateway Pattern](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/gateway) - HIGH confidence

---
*Architecture research for: E-commerce Microservices*
*Researched: 2026-01-29*
