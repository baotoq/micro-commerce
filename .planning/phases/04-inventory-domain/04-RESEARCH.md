# Phase 4: Inventory Domain - Research

**Researched:** 2026-02-07
**Domain:** Inventory tracking, stock reservation, optimistic concurrency (DDD + CQRS in .NET modular monolith)
**Confidence:** HIGH

## Summary

The Inventory domain follows the exact same modular monolith patterns already established in the Catalog module: own DbContext with schema isolation (`inventory` schema), CQRS via MediatR, DDD aggregates using the existing `BaseAggregateRoot<TId>` base class, domain events dispatched via MassTransit through the existing `DomainEventInterceptor`, and Minimal API endpoints. No new packages are needed -- everything required (EF Core, MediatR, MassTransit, FluentValidation) is already in the project.

The core technical challenges are: (1) optimistic concurrency on stock updates using PostgreSQL's `xmin` system column as a row version token, (2) a reservation pattern with TTL-based expiry, (3) cross-module event consumption (consuming `ProductCreatedDomainEvent` from Catalog to auto-create stock items), and (4) computing "available quantity" as total stock minus active non-expired reservations.

**Primary recommendation:** Follow the Catalog module structure exactly. Use `xmin`-based optimistic concurrency via EF Core's `IsRowVersion()`. Model reservations as a separate entity with `ExpiresAt` timestamp, and compute available stock with a query that subtracts active reservations from total quantity.

## Standard Stack

### Core (Already in Project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core (Npgsql) | 10.0.0 | Data access, migrations, concurrency | Already configured for schema-per-module |
| MediatR | 13.1.0 | CQRS command/query dispatch | Already wired with validation pipeline |
| MassTransit | 9.0.0 | Domain event pub/sub, consumer pattern | Already configured with Azure Service Bus + EF outbox |
| FluentValidation | 12.1.1 | Input validation | Already auto-discovered from assembly |
| Aspire.Npgsql.EFCore | 13.1.0 | DbContext registration via Aspire | Already used for other modules |

### Supporting (Already in Project)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| BuildingBlocks.Common | local | BaseAggregateRoot, StronglyTypedId, ValueObject, DomainEvent | All domain entities and events |

### No New Dependencies Needed
The entire inventory domain can be built with the existing package set. No new NuGet packages or npm packages required (shadcn/ui components like Badge, Table, Dialog, Input are already installed).

## Architecture Patterns

### Recommended Project Structure
```
Features/Inventory/
  Domain/
    Entities/
      StockItem.cs              # Aggregate root - tracks quantity per product
      StockReservation.cs       # Entity - time-bound reservation
      StockAdjustment.cs        # Entity - audit log of adjustments
    ValueObjects/
      StockItemId.cs            # Strongly-typed ID
      ReservationId.cs          # Strongly-typed ID
      AdjustmentId.cs           # Strongly-typed ID
      Quantity.cs               # Value object wrapping int, non-negative
    Events/
      StockReservedDomainEvent.cs
      StockReleasedDomainEvent.cs
      StockAdjustedDomainEvent.cs
      StockLowDomainEvent.cs
  Application/
    Commands/
      AdjustStock/
        AdjustStockCommand.cs
        AdjustStockCommandHandler.cs
        AdjustStockCommandValidator.cs
      ReserveStock/
        ReserveStockCommand.cs
        ReserveStockCommandHandler.cs
        ReserveStockCommandValidator.cs
      ReleaseReservation/
        ReleaseReservationCommand.cs
        ReleaseReservationCommandHandler.cs
      ReleaseExpiredReservations/
        ReleaseExpiredReservationsCommand.cs
        ReleaseExpiredReservationsCommandHandler.cs
    Queries/
      GetStockByProductId/
        GetStockByProductIdQuery.cs
        GetStockByProductIdQueryHandler.cs
        StockInfoDto.cs
      GetStockLevels/
        GetStockLevelsQuery.cs
        GetStockLevelsQueryHandler.cs
      GetAdjustmentHistory/
        GetAdjustmentHistoryQuery.cs
        GetAdjustmentHistoryQueryHandler.cs
        AdjustmentDto.cs
    Consumers/
      ProductCreatedConsumer.cs   # MassTransit consumer for auto-creating StockItem
  Infrastructure/
    InventoryDbContext.cs         # Already exists (skeleton)
    Configurations/
      StockItemConfiguration.cs
      StockReservationConfiguration.cs
      StockAdjustmentConfiguration.cs
    Migrations/
  InventoryEndpoints.cs           # Minimal API endpoints
```

### Pattern 1: StockItem Aggregate with Encapsulated Business Logic
**What:** The StockItem aggregate owns all stock mutation logic. Reservations and adjustments go through StockItem methods that enforce invariants.
**When to use:** All stock operations.
**Example:**
```csharp
public sealed class StockItem : BaseAggregateRoot<StockItemId>
{
    private readonly List<StockReservation> _reservations = [];

    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    [Timestamp]
    public uint Version { get; private set; } // xmin concurrency token

    public int AvailableQuantity =>
        QuantityOnHand - _reservations
            .Where(r => r.ExpiresAt > DateTimeOffset.UtcNow)
            .Sum(r => r.Quantity);

    public static StockItem Create(Guid productId)
    {
        var item = new StockItem(StockItemId.New())
        {
            ProductId = productId,
            QuantityOnHand = 0
        };
        return item;
    }

    public void AdjustStock(int adjustment, string? reason, string? adjustedBy)
    {
        var newQuantity = QuantityOnHand + adjustment;
        if (newQuantity < 0)
            throw new InvalidOperationException("Stock cannot go below zero.");

        QuantityOnHand = newQuantity;
        AddDomainEvent(new StockAdjustedDomainEvent(Id.Value, ProductId, adjustment, QuantityOnHand));

        if (QuantityOnHand <= 10) // Low stock threshold
            AddDomainEvent(new StockLowDomainEvent(Id.Value, ProductId, QuantityOnHand));
    }

    public ReservationId Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock available.");

        var reservation = StockReservation.Create(Id, quantity, TimeSpan.FromMinutes(15));
        _reservations.Add(reservation);

        AddDomainEvent(new StockReservedDomainEvent(Id.Value, ProductId, reservation.Id.Value, quantity));
        return reservation.Id;
    }

    public void ReleaseReservation(ReservationId reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null) return;

        _reservations.Remove(reservation);
        AddDomainEvent(new StockReleasedDomainEvent(Id.Value, ProductId, reservationId.Value, reservation.Quantity));
    }
}
```

### Pattern 2: MassTransit Consumer for Cross-Module Events
**What:** Consume `ProductCreatedDomainEvent` from Catalog module to auto-create a StockItem with quantity 0.
**When to use:** When Inventory module needs to react to events from other modules.
**Example:**
```csharp
// Source: Existing codebase pattern (IDomainEventHandler remarks + DomainEventInterceptor)
public sealed class ProductCreatedConsumer : IConsumer<ProductCreatedDomainEvent>
{
    private readonly InventoryDbContext _context;

    public ProductCreatedConsumer(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<ProductCreatedDomainEvent> context)
    {
        var productId = context.Message.ProductId;

        var exists = await _context.StockItems
            .AnyAsync(s => s.ProductId == productId);

        if (exists) return; // Idempotency

        var stockItem = StockItem.Create(productId);
        _context.StockItems.Add(stockItem);
        await _context.SaveChangesAsync();
    }
}
```

### Pattern 3: Optimistic Concurrency with xmin
**What:** PostgreSQL's `xmin` system column as row version token. EF Core detects concurrent modifications and throws `DbUpdateConcurrencyException`.
**When to use:** StockItem entity to prevent lost updates during concurrent stock adjustments or reservations.
**Example:**
```csharp
// Source: https://www.npgsql.org/efcore/modeling/concurrency.html
// In StockItemConfiguration:
builder.Property(s => s.Version)
    .IsRowVersion(); // Maps to xmin automatically with Npgsql provider

// In handler - catch and retry:
try
{
    stockItem.AdjustStock(command.Adjustment, command.Reason, command.AdjustedBy);
    await _context.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateConcurrencyException)
{
    // Reload and retry, or return conflict
    throw new ConflictException("Stock was modified concurrently. Please retry.");
}
```

### Pattern 4: Separate Inventory API Endpoint for Stock Info
**What:** Expose stock info via `/api/inventory/stock/{productId}` rather than embedding in Catalog product response.
**When to use:** Respects module boundary -- Catalog does not know about Inventory.
**Rationale:** In a modular monolith designed for microservice extraction, having Catalog return inventory data creates a coupling that would need to be unwound later. The frontend makes a parallel call. This is a Claude's Discretion item -- this approach is recommended because:
  - Clean module boundary (Inventory owns its data)
  - No cross-schema joins needed
  - Frontend can cache/poll stock independently
  - When extracting to microservices, no API changes needed

### Anti-Patterns to Avoid
- **Cross-schema joins:** Do NOT join inventory tables from CatalogDbContext. Each module queries its own schema.
- **Direct entity references across modules:** Inventory references `ProductId` as a raw `Guid`, not as a navigation property to `Product`.
- **Rich reservation entities:** Keep `StockReservation` as a simple entity owned by `StockItem`, not a separate aggregate.
- **Storing available quantity:** Compute `AvailableQuantity` dynamically from `QuantityOnHand - active reservations`. Never store it -- it would create a consistency problem.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Optimistic concurrency | Manual version checking with WHERE clauses | EF Core `IsRowVersion()` + `xmin` | Npgsql provider handles xmin mapping automatically; manual approach misses edge cases |
| Domain event dispatch | Custom event bus or in-process mediator | MassTransit `IPublishEndpoint` via existing `DomainEventInterceptor` | Already configured with outbox, retry, dead-letter |
| Reservation expiry cleanup | Custom Timer/Thread | Hosted service with periodic `IServiceScopeFactory` | Simple, testable, uses DI correctly |
| Concurrency token storage | Manual rowversion column | `[Timestamp]` attribute or `.IsRowVersion()` | Npgsql maps this to `xmin` automatically -- no extra column needed |

**Key insight:** The entire infrastructure for domain events, concurrency, and persistence is already wired up in this project. The Inventory module just needs to follow the established patterns.

## Common Pitfalls

### Pitfall 1: Forgetting Idempotency in MassTransit Consumers
**What goes wrong:** `ProductCreatedConsumer` creates duplicate StockItems if the message is retried.
**Why it happens:** MassTransit guarantees at-least-once delivery, not exactly-once.
**How to avoid:** Always check if the entity already exists before creating. Use `AnyAsync` check before insert.
**Warning signs:** Duplicate key exceptions in logs, multiple StockItems for same ProductId.

### Pitfall 2: Expired Reservations Not Cleaned Up
**What goes wrong:** Expired reservations accumulate in the database, slowing queries and wasting space.
**Why it happens:** TTL-based expiry requires an active cleanup process -- records don't delete themselves.
**How to avoid:** Create a `BackgroundService` / hosted service that periodically removes expired reservations (e.g., every 1-2 minutes). Also, filter out expired reservations in all queries (defensive).
**Warning signs:** Growing reservation table size, slow available-quantity calculations.

### Pitfall 3: DbUpdateConcurrencyException Not Handled
**What goes wrong:** Unhandled exception returns 500 to client instead of a meaningful concurrency conflict.
**Why it happens:** EF Core throws `DbUpdateConcurrencyException` when xmin doesn't match.
**How to avoid:** Catch in handler, throw `ConflictException` (409 status via existing `GlobalExceptionHandler`).
**Warning signs:** 500 errors during concurrent admin adjustments.

### Pitfall 4: N+1 Queries When Loading Reservations
**What goes wrong:** Loading StockItem without `.Include(s => s.Reservations)` causes lazy-load or missing data.
**Why it happens:** EF Core doesn't load navigation properties by default.
**How to avoid:** Always `.Include(s => s.Reservations.Where(r => r.ExpiresAt > DateTimeOffset.UtcNow))` when computing available quantity. Use filtered includes to skip expired ones.
**Warning signs:** AvailableQuantity equals QuantityOnHand even when active reservations exist.

### Pitfall 5: StockAdjustment History Not Saved Within Same Transaction
**What goes wrong:** Stock adjusts successfully but audit record is lost if saved separately.
**Why it happens:** Creating the adjustment in a separate operation outside the SaveChanges call.
**How to avoid:** Create `StockAdjustment` entity in the same handler, add to DbContext, single `SaveChangesAsync`. The adjustment is a separate entity (not owned by StockItem aggregate) to keep the aggregate focused on stock logic.
**Warning signs:** Missing audit trail entries, inconsistent adjustment history.

## Code Examples

### StockReservation Entity
```csharp
public sealed class StockReservation
{
    public ReservationId Id { get; private set; }
    public StockItemId StockItemId { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private StockReservation() { } // EF Core

    public static StockReservation Create(StockItemId stockItemId, int quantity, TimeSpan ttl)
    {
        return new StockReservation
        {
            Id = ReservationId.New(),
            StockItemId = stockItemId,
            Quantity = quantity,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl)
        };
    }

    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
}
```

### StockAdjustment Audit Entity
```csharp
public sealed class StockAdjustment
{
    public AdjustmentId Id { get; private set; }
    public StockItemId StockItemId { get; private set; }
    public int Adjustment { get; private set; }       // +10 or -5
    public int QuantityAfter { get; private set; }     // Resulting quantity
    public string? Reason { get; private set; }
    public string? AdjustedBy { get; private set; }    // User ID or name
    public DateTimeOffset CreatedAt { get; private set; }

    public static StockAdjustment Create(
        StockItemId stockItemId, int adjustment, int quantityAfter,
        string? reason, string? adjustedBy)
    {
        return new StockAdjustment
        {
            Id = AdjustmentId.New(),
            StockItemId = stockItemId,
            Adjustment = adjustment,
            QuantityAfter = quantityAfter,
            Reason = reason,
            AdjustedBy = adjustedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
```

### EF Core Configuration for StockItem with xmin
```csharp
public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new StockItemId(value));

        builder.Property(s => s.ProductId).IsRequired();
        builder.HasIndex(s => s.ProductId).IsUnique(); // One StockItem per product

        builder.Property(s => s.QuantityOnHand).IsRequired();

        // xmin-based optimistic concurrency
        builder.Property(s => s.Version).IsRowVersion();

        // Reservations as owned collection
        builder.HasMany(s => s.Reservations)
            .WithOne()
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Reservation Expiry Background Service
```csharp
public sealed class ReservationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReservationCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var expired = await context.StockReservations
                .Where(r => r.ExpiresAt <= DateTimeOffset.UtcNow)
                .ToListAsync(stoppingToken);

            if (expired.Count > 0)
            {
                context.StockReservations.RemoveRange(expired);
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
```

### Inventory Endpoints Pattern
```csharp
public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/inventory")
            .WithTags("Inventory");

        group.MapGet("/stock/{productId:guid}", GetStockByProductId);
        group.MapGet("/stock", GetStockLevels);  // Batch: ?productIds=guid1,guid2
        group.MapPost("/stock/{productId:guid}/adjust", AdjustStock);
        group.MapPost("/stock/{productId:guid}/reserve", ReserveStock);
        group.MapDelete("/reservations/{reservationId:guid}", ReleaseReservation);
        group.MapGet("/stock/{productId:guid}/adjustments", GetAdjustmentHistory);

        return endpoints;
    }
}
```

### Frontend Stock Display Pattern
```tsx
// Fetch stock info separately from product data
const stockInfo = await fetch(`/api/inventory/stock/${product.id}`);
// Returns: { productId, quantityOnHand, availableQuantity, isInStock, isLowStock }

// Display logic based on CONTEXT decisions:
// - availableQuantity > 10: "In Stock"
// - availableQuantity 1-10: "Only X left!"
// - availableQuantity 0: "Out of Stock" + hide Add to Cart
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual rowversion columns | `xmin` system column via `IsRowVersion()` | Npgsql 5.0+ | No extra column needed, automatic |
| In-process domain event handlers (INotificationHandler) | MassTransit IConsumer with outbox | Already in project (IDomainEventHandler marked obsolete) | Reliable delivery, retry, dead-letter |
| Timer-based scheduled cleanup | BackgroundService / IHostedService | .NET Core 2.1+ (standard now) | Clean DI integration, graceful shutdown |

**Deprecated/outdated:**
- `ForNpgsqlUseXminAsConcurrencyToken()` -- older API. Use `Property(x).IsRowVersion()` or `[Timestamp]` attribute instead.
- `IDomainEventHandler<T>` -- marked `[Obsolete]` in this codebase. Use MassTransit `IConsumer<T>` instead.

## Open Questions

1. **Batch stock fetching for product grid**
   - What we know: Storefront product grid shows multiple products; each needs stock status.
   - What's unclear: Whether to use a batch endpoint (`/stock?productIds=id1,id2,...`) or individual calls.
   - Recommendation: Implement a batch endpoint that accepts multiple product IDs and returns stock info for all. The product grid handler can call this once after fetching products. More efficient than N individual calls.

2. **Reservation re-reserve on expiry mid-checkout**
   - What we know: Context says "silently attempt to re-reserve; only block if truly out of stock."
   - What's unclear: The checkout flow itself is Phase 7, so reservation creation/re-creation will be triggered from there.
   - Recommendation: In Phase 4, build the `ReserveStock` and `ReleaseReservation` commands. The "re-reserve on expiry" logic belongs in Phase 7's checkout flow, which will call these commands.

3. **Admin user identity for adjustment audit**
   - What we know: Keycloak is configured, JWT auth is in place.
   - What's unclear: How to extract admin user identity in the command handler.
   - Recommendation: Pass `ClaimsPrincipal` info from the endpoint layer into the command (e.g., `AdjustedBy` from `user.FindFirst("preferred_username")?.Value`). Keep it simple -- just a string.

## Sources

### Primary (HIGH confidence)
- Existing codebase: Catalog module structure, BuildingBlocks base classes, Program.cs configuration, DomainEventInterceptor
- [Npgsql Concurrency Tokens](https://www.npgsql.org/efcore/modeling/concurrency.html) - xmin usage, IsRowVersion() API
- [EF Core Concurrency Handling](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) - DbUpdateConcurrencyException pattern

### Secondary (MEDIUM confidence)
- [MassTransit Consumers](https://masstransit.io/documentation/configuration/consumers) - IConsumer pattern
- [MassTransit Entity Framework](https://masstransit.io/documentation/configuration/persistence/entity-framework) - Outbox pattern

### Tertiary (LOW confidence)
- None -- all findings verified against codebase or official docs

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, verified in csproj and Program.cs
- Architecture: HIGH - Follows exact patterns from existing Catalog module
- Pitfalls: HIGH - Based on well-known EF Core and MassTransit patterns, verified against official docs
- Concurrency: HIGH - Verified xmin approach against Npgsql official documentation

**Research date:** 2026-02-07
**Valid until:** 2026-03-07 (stable -- all dependencies are already locked in project)
