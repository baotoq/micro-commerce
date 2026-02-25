# Phase 21: Adoption - Full Building Block Integration - Research

**Researched:** 2026-02-25
**Domain:** .NET DDD building block adoption, EF Core migrations, ASP.NET Core OpenAPI schema transformers
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **Result pattern expansion:** Claude selects which additional command handlers (beyond Phase 17 pilot: UpdateOrderStatus, AdjustStock) to migrate to Result pattern
- **Selection criteria:** handlers with meaningful business rule validation that can fail for domain reasons, not just input validation
- **Endpoint mapping:** use specific HTTP status codes for different failure types (409 Conflict for state violations, 422 for business rule failures, 404 for not found)
- **Non-migrated handlers:** leave as-is with no TODO comments — ADOPT-F01 already tracks full adoption as a future requirement

### Claude's Discretion

- Which specific command handlers to migrate to Result (2+ additional beyond pilot)
- Whether to add integration tests for newly-migrated Result handlers
- Specification pattern coverage — which queries beyond Phase 19 specs should use Specification
- Audit and concurrency scope — which aggregates get AuditableAggregateRoot, whether any entities beyond Order/Cart/StockItem need concurrency tokens
- OpenAPI schema filter design for StronglyTypedId (primitive display) and SmartEnum (string display)
- Migration sequencing (module-by-module vs building-block-by-building-block)

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| ADOPT-01 | Migrate all child entities across 7 modules to Entity base class | Entity<TId> base class exists; CartItem, OrderItem, StockReservation, StockAdjustment, Address are the candidates |
| ADOPT-02 | Migrate all aggregates to AuditableAggregateRoot or apply IAuditable where appropriate | AuditableAggregateRoot<TId> exists; aggregates with manual CreatedAt/UpdatedAt properties are candidates |
| ADOPT-03 | Migrate existing optimistic concurrency (Order, Cart, StockItem) from xmin to IConcurrencyToken with explicit Version column | IConcurrencyToken + ConcurrencyInterceptor + ConcurrencyTokenConvention all exist; xmin [Timestamp] uint must change to int Version |
| ADOPT-04 | Migrate all StronglyTypedId types to use source generator, remove manual converter configurations | Vogen adoption complete in Phase 16.1; VogenEfCoreConverters.cs already has all 13 IDs registered; this requirement is already DONE |
| ADOPT-05 | Adopt Result pattern in 2+ command handlers as pilot (e.g., SubmitOrder, ProcessPayment) | UpdateOrderStatus + AdjustStock already migrated (Phase 17); need 2+ more from remaining handlers |
| ADOPT-06 | Apply Specification pattern to complex catalog and ordering queries | Catalog and Ordering specs already exist (Phase 19); remaining queries in Inventory/Profiles/Reviews/Wishlists may need specs |
| ADOPT-07 | All existing tests pass after migration with no regressions | 178 tests pass currently; all changes require DB migration for schema changes |
| MOD-04 | OpenAPI schema filters for StronglyTypedId (primitive display) and Enumeration (string display) | ASP.NET Core 10 IOpenApiSchemaTransformer API confirmed; Vogen types are record structs |
</phase_requirements>

---

## Summary

Phase 21 applies all building blocks created in Phases 15-20 across all 7 feature modules. The research reveals that the work is significantly less than the phase description suggests — ADOPT-04 (StronglyTypedId migration) is already complete since Phase 16.1 fully adopted Vogen with the VogenEfCoreConverters class covering all 13 ID types. The primary work items are: (1) migrating child entities to Entity base, (2) migrating aggregates to AuditableAggregateRoot or adding IAuditable, (3) converting xmin-based optimistic concurrency to IConcurrencyToken, (4) selecting and migrating 2+ more command handlers to Result pattern, (5) evaluating Specification pattern coverage for remaining query handlers, and (6) implementing OpenAPI schema transformers for Vogen IDs and SmartEnums.

The most technically complex item is ADOPT-03 (xmin to IConcurrencyToken migration) because it requires EF Core migrations across 5 DbContexts (Cart, Order, Inventory, Profiles, Reviews, Wishlists). The xmin-based `[Timestamp] public uint Version` must become `public int Version : IConcurrencyToken` with the ConcurrencyInterceptor handling version increments. The existing ConcurrencyInterceptor and ConcurrencyTokenConvention are already implemented and ready. The OpenAPI schema transformer work (MOD-04) uses the ASP.NET Core 10 `IOpenApiSchemaTransformer` interface via `AddSchemaTransformer<T>()` on `AddOpenApi()` options — no additional packages needed.

**Primary recommendation:** Sequence migration as building-block-by-building-block across all 7 modules: (1) Entity adoption, (2) Auditable migration, (3) IConcurrencyToken migration + migrations, (4) Result pattern expansion, (5) Specification coverage audit, (6) OpenAPI transformers. Each wave is independently testable.

---

## Standard Stack

### Core (already installed, no new packages needed)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `BuildingBlocks.Common` | project ref | Entity, IAuditable, IConcurrencyToken, AuditableAggregateRoot | All building blocks already implemented |
| `Microsoft.AspNetCore.OpenApi` | 10.0.2 | OpenAPI schema transformer API | Already in MicroCommerce.ApiService.csproj |
| `FluentResults` | 4.0.0 | Result pattern in handlers | Already installed, used in Phase 17 pilots |
| `Ardalis.Specification.EntityFrameworkCore` | 9.3.1 | Specification pattern with EF Core | Already installed, used in Phase 19 |
| `Vogen` | 8.0.4 | Source-generated strongly-typed IDs | Already installed; ADOPT-04 is done |

### No New Packages Required

All tools needed for Phase 21 are already installed. This is purely an adoption/migration phase.

---

## Architecture Patterns

### ADOPT-01: Child Entity Migration

**What:** 5 child entity classes need to inherit `Entity<TId>` from BuildingBlocks.Common.

**Child entities identified:**
- `CartItem` — owned by Cart aggregate; currently has `CartItemId Id` as plain property
- `OrderItem` — owned by Order aggregate; currently has `OrderItemId Id` as plain property
- `StockReservation` — owned by StockItem aggregate; currently has `ReservationId Id` as plain property
- `StockAdjustment` — persisted independently (not truly owned); currently has `AdjustmentId Id` as plain property
- `Address` — owned by UserProfile aggregate via `OwnsMany`; currently has `AddressId Id` as plain property

**Pattern:**
```csharp
// Before
public sealed class CartItem
{
    public CartItemId Id { get; private set; }
    // ...
    private CartItem() { }
}

// After
public sealed class CartItem : Entity<CartItemId>
{
    // Id is now inherited from Entity<CartItemId>
    // private set replaced by protected init from base
    // EF Core constructor: protected CartItem() : base() {}
}
```

**Important nuance for EF Core:** Entity<TId> has `protected Entity()` for ORM and `protected Entity(TId id)`. Child entities with `private CartItem()` EF constructors must change to `protected CartItem() : base()`. The `Id` property changes from `{ get; private set; }` to the inherited `{ get; protected init; }` — assignments like `Id = CartItemId.New()` in factory methods must use the base constructor or the inherited property directly.

**Address special case:** Address is in `ValueObjects/` folder but has an `AddressId Id` and owns state mutation (`SetAsDefault`, `ClearDefault`). It is semantically an entity (has identity, mutable state), not a true value object. Migrating to `Entity<AddressId>` is the correct DDD move. Its EF Core mapping is via `OwnsMany` which fully supports entity-typed owned items.

### ADOPT-02: AuditableAggregateRoot Migration

**What:** Aggregates that currently have manually-declared `CreatedAt`/`UpdatedAt` properties should inherit from `AuditableAggregateRoot<TId>`.

**Aggregates with manual timestamps:**
- `Category` — has `public DateTimeOffset CreatedAt { get; private set; }` and `public DateTimeOffset? UpdatedAt { get; private set; }` (UpdatedAt is nullable here, but IAuditable requires non-nullable)
- `Product` — has `public DateTimeOffset CreatedAt { get; private set; }` and `public DateTimeOffset? UpdatedAt { get; private set; }` (same nullable issue)
- `Review` — has `public DateTimeOffset CreatedAt { get; private set; }` and `public DateTimeOffset UpdatedAt { get; private set; }` — directly compatible
- `UserProfile` — has `public DateTimeOffset CreatedAt { get; private set; }` and `public DateTimeOffset UpdatedAt { get; private set; }` — directly compatible but also has IConcurrencyToken xmin

**Nullable UpdatedAt conflict:** Category and Product use `DateTimeOffset? UpdatedAt` (nullable, set on update). AuditableAggregateRoot has `DateTimeOffset UpdatedAt { get; set; }` (non-nullable). Migration needs to make UpdatedAt non-nullable on these aggregates. The AuditInterceptor will auto-set both on Added and UpdatedAt on Modified — so manual `UpdatedAt = DateTimeOffset.UtcNow` calls in domain methods can be removed (interceptor handles it).

**Aggregates that do NOT need AuditableAggregateRoot:**
- `Cart` — has `CreatedAt`, `LastModifiedAt` (different name), `ExpiresAt` — LastModifiedAt name differs from IAuditable.UpdatedAt; need to decide: rename to UpdatedAt and adopt, or leave as custom. Claude's discretion: rename to UpdatedAt and adopt AuditableAggregateRoot, removing the redundant Touch() timestamp logic.
- `StockItem` — no timestamps at all; no audit needed
- `WishlistItem` — has `AddedAt` (not CreatedAt/UpdatedAt); is not an aggregate root
- `Order` — has `CreatedAt` only (no UpdatedAt per se); may not need auditable

**AuditInterceptor interaction:** The `AuditInterceptor.UpdateAuditFields` sets `CreatedAt` and `UpdatedAt` on `EntityState.Added`, `UpdatedAt` on `EntityState.Modified`. Once aggregates inherit `AuditableAggregateRoot`, all manual timestamp assignments in factory methods and domain methods should be removed to avoid double-setting.

### ADOPT-03: xmin to IConcurrencyToken Migration

**What:** Replace PostgreSQL xmin-based concurrency with IConcurrencyToken explicit Version column.

**Entities with `[Timestamp] public uint Version`:**
- `Cart` (CartDbContext)
- `Order` (OrderingDbContext)
- `StockItem` (InventoryDbContext)
- `UserProfile` (ProfilesDbContext)
- `Review` (ReviewsDbContext)
- `WishlistItem` (WishlistsDbContext) — note: WishlistItem is not an aggregate root

**Migration per entity:**
```csharp
// Before
[Timestamp]
public uint Version { get; private set; }

// After — implement IConcurrencyToken
public int Version { get; set; }  // { get; set; } required for interceptor
```

The entity also needs to explicitly implement `IConcurrencyToken`. The `ConcurrencyTokenConvention` already detects `IConcurrencyToken` types and marks the `Version` property as a concurrency token.

**EF Core migration impact:** Each affected DbContext needs a new migration. The xmin column was mapped via `[Timestamp]` which EF Core maps to the PostgreSQL `xmin` system column (not a real column in the table). Switching to IConcurrencyToken adds a real `version` (snake_case) integer column to each table. This is a breaking schema change — migrations will `DROP COLUMN xmin` (though xmin is a system column, EF removes the mapping) and `ADD COLUMN version INTEGER NOT NULL DEFAULT 1`.

**Affected migrations (one per DbContext):**
- CartDbContext: `carts` table
- OrderingDbContext: `orders` table
- InventoryDbContext: `stock_items` table
- ProfilesDbContext: `user_profiles` table
- ReviewsDbContext: `reviews` table
- WishlistsDbContext: `wishlist_items` table

**DbUpdateConcurrencyException handling:** Already handled by GlobalExceptionHandler or ConflictException pattern. The existing behavior (HTTP 409) is preserved. `StockItem.Reserve` already throws `ConflictException` for insufficient stock — that is separate from concurrency and unchanged.

**WishlistItem special case:** WishlistItem is not an aggregate root and has no domain events. It currently has an xmin version. IConcurrencyToken on a plain entity is valid — the convention detects any type implementing the interface.

### ADOPT-04: StronglyTypedId Source Generator (ALREADY COMPLETE)

**Status: DONE.** VogenEfCoreConverters.cs registers all 13 ID types. No work needed for this requirement.

```
[EfCoreConverter<ProductId>]    [EfCoreConverter<CategoryId>]
[EfCoreConverter<OrderId>]      [EfCoreConverter<OrderItemId>]
[EfCoreConverter<CartId>]       [EfCoreConverter<CartItemId>]
[EfCoreConverter<StockItemId>]  [EfCoreConverter<ReservationId>]
[EfCoreConverter<AdjustmentId>] [EfCoreConverter<UserProfileId>]
[EfCoreConverter<AddressId>]    [EfCoreConverter<ReviewId>]
[EfCoreConverter<WishlistItemId>]
```

All IDs use `[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]` — manual converter configurations were removed in Phase 16.1.

### ADOPT-05: Result Pattern Expansion

**Pilot handlers (Phase 17, already done):**
- `UpdateOrderStatusCommandHandler` — returns `Result`, uses try/catch for `InvalidOperationException`
- `AdjustStockCommandHandler` — returns `Result`, uses try/catch for `InvalidOperationException`

**Recommended additional handlers (Claude's discretion — 2+ more):**

**Candidate 1: `ChangeProductStatusCommandHandler`**
- Currently: `IRequestHandler<ChangeProductStatusCommand, bool>` with `throw new InvalidOperationException` on unknown status
- Has domain state transition logic via `product.Publish()`, `product.Unpublish()`, `product.Archive()`
- Product state transitions can fail: `ProductStatus.TransitionTo()` throws if invalid transition
- **Recommended migration:** Return `Result` instead of `bool`, catch `InvalidOperationException` from `TransitionTo`, return `Result.Fail`. Map to 422 at endpoint.
- Endpoint HTTP codes: 404 for not found, 422 for invalid transition

**Candidate 2: `UpdateCartItemCommandHandler`**
- Currently: `IRequestHandler<UpdateCartItemCommand, Unit>` with `throw new InvalidOperationException` for item-not-found
- `cart.UpdateItemQuantity` throws `InvalidOperationException` if item not found
- **Note:** "item not found in cart" is a business scenario (could be stale UI) that deserves a 422 or 404, not a 500 from uncaught exception
- **Recommended migration:** Return `Result`, catch from domain methods, return Result.Fail
- Endpoint HTTP codes: 404 for cart not found, 422 for item not found in cart

**Anti-patterns to avoid:** Do NOT migrate handlers where failures are purely data-not-found (use NotFoundException → 404 pattern already working). Result pattern is for domain rule violations, not simple missing entities.

### ADOPT-06: Specification Pattern Coverage Audit

**Already using Specification (Phase 19):**
- `GetProductsQueryHandler` — uses `GetProductsFilterSpec`
- `GetAllOrdersQueryHandler` — uses `ActiveOrdersSpec`
- `GetOrdersByBuyerQueryHandler` — uses `OrdersByBuyerSpec` + `ActiveOrdersSpec`

**Query handlers NOT using Specification (inventory/profiles/reviews/wishlists):**

Reviewing handlers:
- `GetStockLevelsQueryHandler` — simple `_context.StockItems.AsNoTracking().ToListAsync()` — no complex filtering
- `GetAdjustmentHistoryQueryHandler` — filters by `StockItemId` — simple, one-condition filter
- `GetCartQueryHandler`, `GetCartItemCountQueryHandler` — filter by BuyerId, simple
- `GetOrderDashboardQueryHandler` — complex aggregation/grouping, but EF cannot translate to SQL (done in memory per Phase 20)

**Assessment:** Most of the remaining query handlers have simple single-condition filters that do not benefit meaningfully from Specification encapsulation. Claude's discretion recommends adding Specification only where there is genuine reusability or complexity. The clearest additional candidate is the ordering `GetOrderDashboard` (not applicable — grouping in memory). Given the sparse benefit, no additional Specification adoption is required beyond what exists — ADOPT-06 is satisfied by the existing Phase 19 coverage for "complex catalog and ordering queries."

### MOD-04: OpenAPI Schema Transformers

**What:** ASP.NET Core 10 `IOpenApiSchemaTransformer` API. No Swashbuckle. No additional packages.

**Interface (confirmed from official docs):**
```csharp
// Register in Program.cs
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<VogenIdSchemaTransformer>();
    options.AddSchemaTransformer<SmartEnumSchemaTransformer>();
});
```

**VogenId schema transformer — show Guid as string:**

Vogen IDs are `partial record struct` with a `Value` property. ASP.NET Core's schema generator sees these as complex objects with a `Value` property. The transformer should detect Vogen types and replace the schema with a `string` (uuid format) primitive:

```csharp
// Source: official ASP.NET Core docs IOpenApiSchemaTransformer
internal sealed class VogenIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Vogen types are partial record structs with [ValueObject<T>] attribute
        Type type = context.JsonTypeInfo.Type;
        if (type.GetCustomAttribute<ValueObjectAttribute>() != null
            || IsVogenType(type))
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Properties.Clear();
        }
        return Task.CompletedTask;
    }

    private static bool IsVogenType(Type type)
    {
        // Vogen generates a partial record struct; check for Vogen marker
        // Vogen types implement IVogen<T, TUnderlying> or check for ValueObjectAttribute
        return type.IsValueType && type.GetCustomAttributes()
            .Any(a => a.GetType().Name == "ValueObjectAttribute");
    }
}
```

**Detection strategy for Vogen types:** Vogen-generated types have the `[ValueObject<T>]` attribute applied at compile time. In the schema transformer, `context.JsonTypeInfo.Type` gives the CLR type. Check if the type has `Vogen.ValueObjectAttribute` as a custom attribute. Alternatively, check if the type implements `Vogen.IVogen<TSelf, TValue>` — but attribute detection is simpler.

**SmartEnum schema transformer — show string type:**

SmartEnum types serialize as strings (via `SmartEnumNameConverter`). The schema generator generates them as complex objects. The transformer should replace with a string enum schema:

```csharp
internal sealed class SmartEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (IsSmartEnum(type))
        {
            // Get all valid values for enum documentation
            var allValues = (IEnumerable<SmartEnum<>>)type.GetMethod("get_List")?.Invoke(null, null)!;
            schema.Type = "string";
            schema.Enum = SmartEnum<>.List.Select(s => (IOpenApiAny)new OpenApiString(s.Name)).ToList();
            schema.Properties.Clear();
        }
        return Task.CompletedTask;
    }

    private static bool IsSmartEnum(Type type)
        => type.BaseType?.IsGenericType == true
            && type.BaseType.GetGenericTypeDefinition() == typeof(SmartEnum<>);
}
```

**Registration in Program.cs:**
```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<VogenIdSchemaTransformer>();
    options.AddSchemaTransformer<SmartEnumSchemaTransformer>();
});
```

Place these transformer classes in `Common/OpenApi/` or directly in the area near `Program.cs`.

---

## Detailed Migration Inventory

### ADOPT-01: Child Entity Migration Checklist

| Entity | Feature | Current Base | Target Base | EF Mapping |
|--------|---------|-------------|-------------|------------|
| `CartItem` | Cart | none | `Entity<CartItemId>` | Owned via HasMany, table `cart_items` |
| `OrderItem` | Ordering | none | `Entity<OrderItemId>` | Owned via HasMany, table `order_items` |
| `StockReservation` | Inventory | none | `Entity<ReservationId>` | Owned via HasMany, table `stock_reservations` |
| `StockAdjustment` | Inventory | none | `Entity<AdjustmentId>` | Standalone DbSet, table `stock_adjustments` |
| `Address` | Profiles | none | `Entity<AddressId>` | OwnsMany, table `profiles.addresses` |

**No EF migration needed for ADOPT-01.** The Entity<TId> base class only adds an `Id` property that already exists in these entities. EF Core configurations remain unchanged. The schema is not affected.

### ADOPT-02: AuditableAggregateRoot Migration Checklist

| Aggregate | Feature | CurrentManual Timestamps | Action | Note |
|-----------|---------|--------------------------|--------|------|
| `Category` | Catalog | `CreatedAt` + `DateTimeOffset? UpdatedAt` | Migrate to `AuditableAggregateRoot<CategoryId>` | UpdatedAt becomes non-nullable; remove manual assignments |
| `Product` | Catalog | `CreatedAt` + `DateTimeOffset? UpdatedAt` | Migrate to `AuditableAggregateRoot<ProductId>` | UpdatedAt becomes non-nullable; remove manual assignments |
| `Review` | Reviews | `CreatedAt` + `UpdatedAt` | Migrate to `AuditableAggregateRoot<ReviewId>` | Direct match; remove manual assignments |
| `UserProfile` | Profiles | `CreatedAt` + `UpdatedAt` | Migrate to `AuditableAggregateRoot<UserProfileId>` | Direct match; also has IConcurrencyToken (address in ADOPT-03) |
| `Cart` | Cart | `CreatedAt` + `LastModifiedAt` | Migrate to `AuditableAggregateRoot<CartId>`, rename `LastModifiedAt` → `UpdatedAt` | Touch() method simplification |
| `Order` | Ordering | `CreatedAt` (no UpdatedAt) | Add `IAuditable` (UpdatedAt will be set by interceptor on status changes) | Or keep as `BaseAggregateRoot` — minimal benefit since Order is append-mostly |

**EF Core migrations needed for ADOPT-02:** Only if nullable `DateTimeOffset? UpdatedAt` becomes non-nullable `DateTimeOffset UpdatedAt`. This is a schema change — the nullable column becomes NOT NULL. A migration with `DEFAULT '1970-01-01' AT TIME ZONE 'UTC'` for existing rows.

**Cart `LastModifiedAt` rename:** Renaming column from `last_modified_at` to `updated_at` requires migration. All existing CartTests reference `LastModifiedAt` — these test references need updating.

### ADOPT-03: IConcurrencyToken Migration Checklist

| Entity | Feature | Current | Target | Migration Required |
|--------|---------|---------|--------|-------------------|
| `Cart` | Cart | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — CartDbContext |
| `Order` | Ordering | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — OrderingDbContext |
| `StockItem` | Inventory | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — InventoryDbContext |
| `UserProfile` | Profiles | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — ProfilesDbContext |
| `Review` | Reviews | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — ReviewsDbContext |
| `WishlistItem` | Wishlists | `[Timestamp] uint Version` | `int Version : IConcurrencyToken` | Yes — WishlistsDbContext |

**Entity change pattern:**
```csharp
// Remove
using System.ComponentModel.DataAnnotations;
// Remove attribute
[Timestamp]
// Change type: uint → int, visibility: private set → public set (interface requirement)
// Before
[Timestamp]
public uint Version { get; private set; }

// After
public int Version { get; set; }  // IConcurrencyToken requires { get; set; }
```

Also add `IConcurrencyToken` to the class declaration and remove `using System.ComponentModel.DataAnnotations` if no longer used.

**Migration SQL pattern (PostgreSQL):**
```sql
-- Remove xmin row version tracking
-- Add real integer version column
ALTER TABLE cart.carts ADD COLUMN version INTEGER NOT NULL DEFAULT 1;
```

EF Core migration will handle this automatically when `dotnet ef migrations add` is run.

### ADOPT-05: Result Pattern Expansion Decision

**Selected handlers for migration:**
1. `ChangeProductStatusCommandHandler` — has domain state transition failures (ProductStatus.TransitionTo throws)
2. `UpdateCartItemCommandHandler` — has domain rule failure (item not found in cart is a business scenario)

**HTTP status code mapping for endpoints (per CONTEXT.md locked decision):**
- 409 Conflict: concurrency/state violations
- 422 Unprocessable Entity: business rule failures (invalid transition, item not found in aggregate context)
- 404 Not Found: entity not found (keep as NotFoundException → GlobalExceptionHandler)

**Endpoint changes:**
```csharp
// CatalogEndpoints.cs - ChangeProductStatus
private static async Task<IResult> ChangeProductStatus(...)
{
    Result result = await sender.Send(command, cancellationToken);
    return result.ToHttpResult();  // uses existing ResultExtensions
}

// CartEndpoints.cs - UpdateCartItem
private static async Task<IResult> UpdateCartItem(...)
{
    Result result = await sender.Send(command, cancellationToken);
    return result.ToHttpResult();
}
```

---

## Common Pitfalls

### Pitfall 1: EF Core Constructor Visibility After Entity<TId> Migration

**What goes wrong:** CartItem, OrderItem etc. have `private CartItem()` for EF Core. Entity<TId> has `protected Entity()`. After migration, the private constructor must become `protected CartItem() : base()` to satisfy EF Core's navigation property loading.

**Why it happens:** EF Core requires a protected or public parameterless constructor to materialize entities. Private constructors work for owned entities loaded via Include, but inheriting Entity<TId> makes EF look for the base chain.

**How to avoid:** Change `private T()` → `protected T() : base()` for all migrated entities.

### Pitfall 2: AuditInterceptor Double-Stamping

**What goes wrong:** If an aggregate already sets `CreatedAt = DateTimeOffset.UtcNow` in its factory method AND the AuditInterceptor also sets it on `EntityState.Added`, the interceptor overwrites the domain-set value (which is fine — both set UtcNow), but the code is redundant.

**Why it happens:** Before AuditableAggregateRoot migration, all timestamps were manually set. After migration, the interceptor handles it.

**How to avoid:** Remove all `CreatedAt = DateTimeOffset.UtcNow` and `UpdatedAt = DateTimeOffset.UtcNow` assignments from domain methods after migrating to AuditableAggregateRoot. Test the AuditInterceptor handles it (integration tests confirm the values are set).

### Pitfall 3: uint Version → int Version Type Change Breaks Tests

**What goes wrong:** Tests that construct Order, Cart, StockItem objects and reference `.Version` (checking it is 0 or some value) will fail if they expect `uint` and get `int`. Also `Version > 0` assertions may need review.

**Why it happens:** `uint` and `int` have different default values and range. `uint` defaults to 0 (unsigned), `int` also defaults to 0 — so default value isn't the issue. The issue is type comparison or casting in test assertions.

**How to avoid:** After changing `uint Version` to `int Version`, search test files for `.Version` usage and verify assertions still compile and are semantically correct.

### Pitfall 4: Vogen Schema Transformer — Detecting `ValueObjectAttribute`

**What goes wrong:** `context.JsonTypeInfo.Type.GetCustomAttribute<ValueObjectAttribute>()` returns null because Vogen generates the attribute as `Vogen.ValueObjectAttribute<T>` (generic), not `Vogen.ValueObjectAttribute` (non-generic). Or the attribute may be on the generated partial — runtime reflection may not see it.

**Why it happens:** Vogen uses generic attribute syntax `[ValueObject<Guid>]` which generates a non-generic `ValueObjectAttribute` on the class, but the exact type name may differ.

**How to avoid:** In the transformer, use a name-based check or check if the type has `IVogen` interface markers. Alternative: check if the type's namespace or name is in the known list of Vogen ID types (simpler, more explicit for this codebase). Or use `type.GetCustomAttributes().Any(a => a.GetType().Name.StartsWith("ValueObject"))`.

### Pitfall 5: OpenAPI Schema Transformer Execution Order

**What goes wrong:** If the Vogen transformer runs after ASP.NET Core's schema compression (which moves repeating schemas to `components/schemas`), the transformer won't see the inline schema.

**Why it happens:** Schema transformers run when schemas are registered, before operations. This is actually fine — but the order matters if both Vogen and SmartEnum transformers need to cooperate.

**How to avoid:** Register Vogen transformer first, SmartEnum transformer second. Both operate on `context.JsonTypeInfo.Type` which is always available. Order confirmed from official docs: schema transformers run in registration order, all before operation transformers.

### Pitfall 6: Cart `LastModifiedAt` Rename Breaks Existing Tests

**What goes wrong:** `CartTests.cs` likely references `cart.LastModifiedAt`. After rename to `UpdatedAt`, these tests fail to compile.

**Why it happens:** Renaming a property requires updating all test references.

**How to avoid:** Search for `LastModifiedAt` in the test project and update all references after the property rename.

---

## Code Examples

### Entity<TId> Migration Pattern

```csharp
// Source: BuildingBlocks.Common/Entity.cs
// Before (CartItem)
public sealed class CartItem
{
    public CartItemId Id { get; private set; }
    public CartId CartId { get; private set; }
    // ...
    private CartItem() { }  // EF Core

    internal static CartItem Create(CartId cartId, ...) =>
        new CartItem { Id = CartItemId.New(), CartId = cartId, ... };
}

// After
public sealed class CartItem : Entity<CartItemId>
{
    public CartId CartId { get; private set; }
    // ...
    protected CartItem() : base() { }  // EF Core

    internal static CartItem Create(CartId cartId, ...) =>
        new CartItem(CartItemId.New()) { CartId = cartId, ... };

    private CartItem(CartItemId id) : base(id) { }
}
```

### AuditableAggregateRoot Migration Pattern

```csharp
// Before (Category)
public sealed class Category : BaseAggregateRoot<CategoryId>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Category Create(...) =>
        new Category(CategoryId.New()) { ..., CreatedAt = DateTimeOffset.UtcNow };

    public void Update(...) { ...; UpdatedAt = DateTimeOffset.UtcNow; }
}

// After
public sealed class Category : AuditableAggregateRoot<CategoryId>
{
    // CreatedAt and UpdatedAt inherited from AuditableAggregateRoot (non-nullable)
    // AuditInterceptor sets them automatically on Add/Modify

    public static Category Create(...) =>
        new Category(CategoryId.New()) { ... };  // No timestamp assignments

    public void Update(...) { ...; }  // No timestamp assignment; interceptor handles it
}
```

### IConcurrencyToken Migration Pattern

```csharp
// Before (Order)
using System.ComponentModel.DataAnnotations;

public sealed class Order : BaseAggregateRoot<OrderId>
{
    [Timestamp]
    public uint Version { get; private set; }
    // ...
}

// After
using MicroCommerce.BuildingBlocks.Common;

public sealed class Order : BaseAggregateRoot<OrderId>, IConcurrencyToken
{
    public int Version { get; set; }  // { get; set; } — IConcurrencyToken requires settable
    // ...
}
```

### OpenAPI Schema Transformer Registration

```csharp
// In Program.cs, update AddOpenApi call:
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<VogenIdSchemaTransformer>();
    options.AddSchemaTransformer<SmartEnumSchemaTransformer>();
});

// In Common/OpenApi/VogenIdSchemaTransformer.cs:
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

internal sealed class VogenIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        // Vogen types: partial record structs with Vogen attributes
        bool isVogenId = type.GetCustomAttributes(inherit: false)
            .Any(a => a.GetType().Name.Contains("ValueObject"));

        if (isVogenId)
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Properties?.Clear();
            schema.AdditionalProperties = null;
        }
        return Task.CompletedTask;
    }
}

// In Common/OpenApi/SmartEnumSchemaTransformer.cs:
using Ardalis.SmartEnum;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

internal sealed class SmartEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        // SmartEnum types extend SmartEnum<T>
        bool isSmartEnum = IsSmartEnumType(type);

        if (isSmartEnum)
        {
            // Replace with string type, populate enum values from SmartEnum.List
            schema.Type = "string";
            schema.Format = null;
            schema.Properties?.Clear();
            // Note: Getting all values requires reflection on the abstract class
            // SmartEnum<T>.List is a static property returning all values
        }
        return Task.CompletedTask;
    }

    private static bool IsSmartEnumType(Type type)
    {
        Type? current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(SmartEnum<>))
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
```

### Result Pattern Expansion — ChangeProductStatus

```csharp
// Before
public sealed class ChangeProductStatusCommandHandler
    : IRequestHandler<ChangeProductStatusCommand, bool>
{
    public async Task<bool> Handle(...) { ... throw new InvalidOperationException(...); }
}

// After
public sealed class ChangeProductStatusCommandHandler
    : IRequestHandler<ChangeProductStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeProductStatusCommand request, CancellationToken ct)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == ProductId.From(request.Id), ct)
            ?? throw new NotFoundException(...);  // NotFoundException still throws — 404

        try
        {
            switch (request.Status.ToLowerInvariant())
            {
                case "published": product.Publish(); break;
                case "draft":     product.Unpublish(); break;
                case "archived":  product.Archive(); break;
                default:          return Result.Fail($"Unknown status: {request.Status}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);  // domain transition failure → 422
        }

        await _context.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
```

---

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 + WebApplicationFactory + Testcontainers.PostgreSql 4.10.0 |
| Config file | none (xunit auto-discovers) |
| Quick run command | `dotnet test src/MicroCommerce.ApiService.Tests/ --filter "Category=Unit" --no-build` |
| Full suite command | `dotnet test src/MicroCommerce.ApiService.Tests/ --no-build` |
| Estimated runtime (unit only) | ~5 seconds |
| Estimated runtime (full suite with integration) | ~60-90 seconds (Testcontainers startup) |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| ADOPT-01 | Child entities compile with Entity<TId> base, EF mappings work | integration | `dotnet test --filter "Category=Integration"` | Yes (existing integration tests run EF queries) |
| ADOPT-02 | AuditInterceptor sets CreatedAt/UpdatedAt on Add/Modify for migrated aggregates | integration | `dotnet test --filter "Category=Integration"` | Partially (endpoint tests verify data roundtrip) |
| ADOPT-03 | Version column persists and increments on save; DbUpdateConcurrencyException on conflict | integration | `dotnet test --filter "Category=Integration"` | No — new tests for version behavior |
| ADOPT-04 | Already complete — no additional tests needed | N/A | N/A | Yes (existing tests already pass) |
| ADOPT-05 | ChangeProductStatus returns Result.Fail for invalid transition; UpdateCartItem returns Result.Fail for missing item | unit/integration | `dotnet test --filter "Category=Unit"` + integration | Partially (OrderTests cover status transitions; no handler-level tests yet) |
| ADOPT-06 | Existing spec-based queries continue to work after any additions | integration | `dotnet test --filter "Category=Integration"` | Yes (CatalogEndpointsTests, OrderingEndpointsTests) |
| ADOPT-07 | All 178 tests remain green after all migration changes | full suite | `dotnet test src/MicroCommerce.ApiService.Tests/ --no-build` | Yes (existing suite) |
| MOD-04 | OpenAPI document contains `string` type for Vogen IDs and SmartEnums | smoke/manual | Run app and inspect `/openapi/v1.json` | No — manual verification |

### Nyquist Sampling Rate

- **Minimum sample interval:** After completing ADOPT-01/02/03 (entity migrations) → run unit tests
- **Full suite trigger:** After each IConcurrencyToken migration (ADOPT-03) due to DB schema changes
- **Phase-complete gate:** Full suite (unit + integration) green before `/gsd:verify-work`
- **Estimated feedback latency per task:** ~5 seconds (unit); ~90 seconds (integration with DB reset)

### Wave 0 Gaps

- [ ] Consider adding a concurrency token unit test for `Order`, `Cart`, `StockItem` post-ADOPT-03 (not strictly required since integration tests cover SaveChanges behavior)
- [ ] Consider adding handler-level integration tests for ChangeProductStatusCommandHandler and UpdateCartItemCommandHandler after Result migration (matching the UpdateOrderStatusHandlerTests pattern)

*(Existing test infrastructure covers all phase requirements — no test framework installation needed)*

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual timestamp in entity (`CreatedAt = UtcNow`) | AuditInterceptor auto-sets via IAuditable | Phase 15 | Remove manual assignments from domain methods |
| PostgreSQL xmin via `[Timestamp] uint` | Explicit `int Version : IConcurrencyToken` | Phase 15/16 convention | Portable, not PostgreSQL-specific |
| Hand-rolled StronglyTypedId hierarchy | Vogen source-generated record structs | Phase 16.1 | VogenEfCoreConverters handles all ID types |
| Swashbuckle ISchemaFilter | ASP.NET Core 10 IOpenApiSchemaTransformer | .NET 9/10 | No Swashbuckle needed; built-in schema transformers |
| `IRequestHandler<T, bool>` for commands | `IRequestHandler<T, Result>` with FluentResults | Phase 17 | 422 for business rule failures instead of 500 |

---

## Open Questions

1. **Cart.LastModifiedAt rename impact on existing Cart tests**
   - What we know: CartTests.cs uses `LastModifiedAt` in time-based assertions
   - What's unclear: Whether the rename is worth the test churn vs leaving Cart's naming as-is
   - Recommendation: Rename to `UpdatedAt` for consistency with IAuditable; update tests — the test count is small and the churn is predictable

2. **Order aggregate — should it get AuditableAggregateRoot?**
   - What we know: Order has `CreatedAt` but no `UpdatedAt`. The order's "update" is a status change, not a general update. Adding `UpdatedAt` via interceptor would make it auto-update on every status change.
   - What's unclear: Whether status change timestamps should be tracked separately (PaidAt exists) vs via generic UpdatedAt
   - Recommendation: Leave Order as `BaseAggregateRoot` (no AuditableAggregateRoot) — the domain-specific timestamps (CreatedAt, PaidAt) are more meaningful than a generic UpdatedAt. Add `IAuditable` only if there is a clear benefit.

3. **WishlistItem — IConcurrencyToken necessity**
   - What we know: WishlistItem has xmin `[Timestamp] uint Version`. It's not an aggregate root; it has a unique index on (UserId, ProductId). Concurrency conflicts on wishlist are low-risk.
   - What's unclear: Whether migrating WishlistItem to IConcurrencyToken is worth the added complexity for a low-risk entity
   - Recommendation: Migrate for consistency (ADOPT-03 says "all existing optimistic concurrency" — WishlistItem has xmin), but keep it minimal.

---

## Sources

### Primary (HIGH confidence)
- Official ASP.NET Core docs (2026-02-02): `IOpenApiSchemaTransformer` API confirmed — `AddSchemaTransformer<T>()`, `TransformAsync(OpenApiSchema, OpenApiSchemaTransformerContext, CancellationToken)`, context has `JsonTypeInfo.Type`
- Project codebase (direct inspection): All building blocks, entity structures, existing patterns confirmed via Read tool

### Secondary (MEDIUM confidence)
- Vogen attribute detection: Based on Vogen 8.0.4 documentation patterns; attribute name `ValueObjectAttribute` on generated types is standard but reflection-based detection may need adjustment

### Tertiary (LOW confidence)
- SmartEnum schema population via reflection: The `List` static property on SmartEnum types is well-known, but reflection in schema transformers may require careful null handling

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages already installed, no new dependencies
- Architecture: HIGH — building blocks fully inspected, migration paths are clear
- Pitfalls: HIGH — derived from direct code inspection of affected entities and existing interceptors
- OpenAPI transformers: MEDIUM — API confirmed from official docs but Vogen attribute detection needs test-time validation

**Research date:** 2026-02-25
**Valid until:** 2026-03-25 (stable .NET 10 API, 30-day validity)
