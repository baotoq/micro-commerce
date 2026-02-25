# Phase 18: Enumeration - Enums with Behavior - Research

**Researched:** 2026-02-24
**Domain:** Ardalis.SmartEnum / C# Domain-Driven Enumerations
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Order State Transitions**
- Strictly linear happy path: Submitted → StockReserved → Paid → Confirmed → Shipped → Delivered
- Failed is reachable only from Submitted and StockReserved (early failures only)
- Cancelled is reachable from Submitted, StockReserved, and Paid (before Confirmed)
- Failed, Cancelled, and Delivered are terminal states (no outbound transitions)
- Only CanTransitionTo() method — no semantic helpers (IsCancellable, IsTerminal, etc.)

**Product Lifecycle Rules**
- Draft → Published (publish)
- Published → Draft (unpublish for edits)
- Published → Archived (retire product)
- Draft → Archived (discard draft)
- Archived is terminal — cannot be re-published or returned to draft
- Transition rules only — no behavior helpers like IsVisibleToCustomers

**Invalid Transition Handling**
- Throw a domain exception (not Result failure) on invalid transitions
- Exception message includes current state, attempted target, and list of valid transitions from current state
- Validation logic lives in the SmartEnum itself via TransitionTo(target) method
- Both paths available: TransitionTo() for validated transitions, direct set for edge cases (admin overrides, data migration)

**Enum Extensibility Pattern**
- Use Ardalis.SmartEnum package (not custom-built)
- SmartEnum types are open (not sealed) — allow potential subclassing
- Generic EF Core ValueConverter and JsonConverter in BuildingBlocks/Common — any future SmartEnum works automatically
- Database storage details are not a concern for this phase — ignore migration complexity

### Claude's Discretion
- SmartEnum integer backing values (preserve existing or reassign)
- Exact placement of converters within BuildingBlocks structure
- How TransitionTo() interacts with existing entity methods
- Test organization and coverage strategy

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PRIM-02 | Enumeration/SmartEnum base (Ardalis.SmartEnum) with EF Core value converter and custom JsonConverter | SmartEnum abstract class pattern confirmed; EF Core requires custom string-based ValueConverter (not ConfigureSmartEnum which stores by int); System.Text.Json requires SmartEnumNameConverter from Ardalis.SmartEnum.SystemTextJson or custom JsonConverter registered globally |
| PRIM-03 | Migrate OrderStatus to SmartEnum with state transition behavior (CanTransitionTo) | Ardalis SmartEnum abstract class + sealed inner classes pattern confirmed from official docs; OrderStatus has 8 states + 12+ guarded transitions across Order entity and consumers; Enum.TryParse<OrderStatus>() calls in query handlers need replacement |
| PRIM-04 | Migrate ProductStatus to SmartEnum with publish/archive behavior | ProductStatus has 3 states + 4 valid transitions; Product.Publish/Unpublish/Archive methods have ad-hoc equality guards that need replacing with TransitionTo(); p.Status.ToString() in LINQ projection needs SmartEnum-aware handling |
</phase_requirements>

## Summary

Phase 18 replaces two plain C# enums (`OrderStatus`, `ProductStatus`) with Ardalis.SmartEnum types that encapsulate state transition rules. The library (version 8.2.0) is well-established with dedicated EF Core and System.Text.Json integration packages. The core pattern uses abstract base classes with sealed inner classes, each overriding an abstract `CanTransitionTo()` method, plus a `TransitionTo()` method on the base that throws on invalid transitions.

The most critical architectural constraint is that the database already stores both status columns as **strings** (confirmed in migration snapshots: `b.Property<string>("Status")`). The `Ardalis.SmartEnum.EFCore` package's `ConfigureSmartEnum()` convention stores by **integer Value** (not Name), making it incompatible with the existing schema. The solution is a generic `SmartEnumStringConverter<TEnum>` in BuildingBlocks that stores by Name. This converter is then registered in `BaseDbContext.ConfigureConventions()` alongside the existing Vogen and other conventions.

JSON serialization requires `Ardalis.SmartEnum.SystemTextJson` and `SmartEnumNameConverter<TEnum, int>` to serialize as a plain string ("Submitted") instead of an object. Without it, the SmartEnum type would serialize as a JSON object with Name/Value properties, breaking the frontend. The converter must be registered globally via ASP.NET Core's `ConfigureHttpJsonOptions` since DTOs include SmartEnum types directly (e.g., `OrderDto.Status` is typed as `OrderStatus`).

**Primary recommendation:** Add `Ardalis.SmartEnum` to BuildingBlocks.Common and `Ardalis.SmartEnum.SystemTextJson` to MicroCommerce.ApiService. Create a generic `SmartEnumStringConverter<TEnum>` for EF Core. Register JSON converter globally in `Program.cs`. Implement OrderStatus and ProductStatus as abstract SmartEnum classes with sealed inner classes.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Ardalis.SmartEnum | 8.2.0 | Base class `SmartEnum<TEnum, TValue>` providing Name, Value, FromName, FromValue, list of all members | Official library from Ardalis; HIGH source reputation on Context7; 62 code snippets available |
| Ardalis.SmartEnum.SystemTextJson | 8.1.0 | `SmartEnumNameConverter<TEnum,TValue>` and `SmartEnumValueConverter<TEnum,TValue>` for System.Text.Json | Required to prevent SmartEnum from serializing as object `{"Name":"...","Value":0}` |
| Ardalis.SmartEnum.EFCore | 8.2.0 | `SmartEnumConverter<TEnum,TValue>` ValueConverter; `ConfigureSmartEnum()` extension | Provides the ValueConverter class — but note: stores by int Value, not by Name |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Ardalis.SmartEnum.ModelBinding | 8.2.0 | Model binding for ASP.NET Core minimal APIs | Only needed if SmartEnum types used directly in route/query parameters (not needed for this phase — endpoints use string parameters) |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Ardalis.SmartEnum | Custom discriminated union (abstract class + sealed types) | More control, more boilerplate; Ardalis.SmartEnum already provides Name, Value, FromName, FromValue, list — no reason to hand-roll |
| SmartEnum abstract inner class pattern | Simple SmartEnum with no inner classes | Inner class pattern is required for per-state behavior (CanTransitionTo must be overridden per state) |
| String-based EF Core ValueConverter | EFCore ConfigureSmartEnum() convention | ConfigureSmartEnum stores by int Value; current DB schema is string; using built-in convention would require a data migration |

**Installation:**
```bash
# BuildingBlocks.Common project
dotnet add package Ardalis.SmartEnum --version 8.2.0

# MicroCommerce.ApiService project
dotnet add package Ardalis.SmartEnum.SystemTextJson --version 8.1.0
```

Note: `Ardalis.SmartEnum.EFCore` is NOT required — we build a custom string converter. EFCore package provides `SmartEnumConverter<TEnum,TValue>` which stores by value (int); we need a string-based converter.

---

## Architecture Patterns

### Recommended Project Structure

```
src/BuildingBlocks/BuildingBlocks.Common/
└── Converters/
    └── SmartEnumStringConverter.cs      # Generic EF Core ValueConverter storing by Name

src/MicroCommerce.ApiService/
├── Features/Ordering/Domain/ValueObjects/
│   └── OrderStatus.cs                   # Replace plain enum with abstract SmartEnum
├── Features/Catalog/Domain/ValueObjects/
│   └── ProductStatus.cs                 # Replace plain enum with abstract SmartEnum
└── Program.cs                           # Register SmartEnumNameConverter globally
```

### Pattern 1: SmartEnum with State Transitions (Abstract Inner Class)

**What:** Abstract base class with sealed inner classes, each implementing `CanTransitionTo()`. A `TransitionTo()` method on the base validates and sets state, throwing `InvalidOperationException` on invalid transitions.
**When to use:** When each state has different allowed next states.
**Example:**
```csharp
// Source: https://github.com/ardalis/SmartEnum README (Context7: /ardalis/smartenum)
public abstract class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Submitted = new SubmittedStatus();
    public static readonly OrderStatus StockReserved = new StockReservedStatus();
    public static readonly OrderStatus Paid = new PaidStatus();
    // ... other states

    private OrderStatus(string name, int value) : base(name, value) { }

    public abstract bool CanTransitionTo(OrderStatus next);

    public void TransitionTo(OrderStatus next)
    {
        if (!CanTransitionTo(next))
        {
            IEnumerable<OrderStatus> valid = List.Where(s => CanTransitionTo(s));
            throw new InvalidOperationException(
                $"Cannot transition from '{Name}' to '{next.Name}'. " +
                $"Valid transitions: {string.Join(", ", valid.Select(s => s.Name))}");
        }
    }

    private sealed class SubmittedStatus : OrderStatus
    {
        public SubmittedStatus() : base("Submitted", 0) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == StockReserved || next == Paid || next == Failed || next == Cancelled;
    }

    private sealed class DeliveredStatus : OrderStatus
    {
        public DeliveredStatus() : base("Delivered", 5) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }
    // ... etc
}
```

### Pattern 2: Generic EF Core String Converter in BuildingBlocks

**What:** A generic `ValueConverter<TEnum, string>` that converts SmartEnum to/from its Name property. Registered in `BaseDbContext.ConfigureConventions()` to apply automatically to all SmartEnum properties.
**When to use:** Any project storing SmartEnum as string (name) in the database.
**Example:**
```csharp
// Source: Derived from Ardalis.SmartEnum.EFCore source; stores by Name not Value
// Location: BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs
public class SmartEnumStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : SmartEnum<TEnum, int>
{
    public SmartEnumStringConverter() : base(
        smartEnum => smartEnum.Name,
        name => SmartEnum<TEnum, int>.FromName(name))
    { }
}
```

Registration in `BaseDbContext.ConfigureConventions()`:
```csharp
// BaseDbContext.cs — add alongside existing conventions
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    base.ConfigureConventions(configurationBuilder);
    configurationBuilder.RegisterAllInVogenEfCoreConverters();
    // New: register SmartEnum string converter for all SmartEnum<TEnum, int> properties
    configurationBuilder.Properties<OrderStatus>()
        .HaveConversion<SmartEnumStringConverter<OrderStatus>>();
    configurationBuilder.Properties<ProductStatus>()
        .HaveConversion<SmartEnumStringConverter<ProductStatus>>();
    // ... other conventions
}
```

Alternatively, using a custom `IModelFinalizingConvention` (consistent with AuditableConvention pattern already used):
```csharp
// SmartEnumConvention.cs — scans model for SmartEnum<,> properties and applies converter
public class SmartEnumConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder builder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in builder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (IsSmartEnumType(property.ClrType))
                {
                    // apply SmartEnumStringConverter
                }
            }
        }
    }
}
```
**Recommendation:** Use `configurationBuilder.Properties<TSmartEnum>()` per-type registration in BaseDbContext for simplicity over a convention scan, since there are only 2 SmartEnum types in this phase.

### Pattern 3: JSON Converter Registration (Global)

**What:** Register `SmartEnumNameConverter<TEnum, int>` globally so all SmartEnum properties in all DTOs serialize as plain strings.
**Why needed:** `OrderDto.Status` is typed as `OrderStatus` (not string), so without a converter, System.Text.Json serializes it as `{"name":"Submitted","value":0,...}`, breaking the frontend.
**Example:**
```csharp
// Program.cs — add after builder construction
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new SmartEnumNameConverter<OrderStatus, int>());
    options.SerializerOptions.Converters.Add(
        new SmartEnumNameConverter<ProductStatus, int>());
});
```
This ensures `"status": "Submitted"` not `"status": {"name":"Submitted","value":0}` in all responses.

### Pattern 4: Entity Method Integration with TransitionTo()

**What:** Replace ad-hoc `if (Status != X) throw` guards in entity methods with calls to `Status.TransitionTo(target)` then direct property assignment.
**Example (Order.cs):**
```csharp
// Before (plain enum):
public void Ship()
{
    if (Status != OrderStatus.Confirmed)
        throw new InvalidOperationException($"Cannot ship when status is '{Status}'.");
    Status = OrderStatus.Shipped;
}

// After (SmartEnum with TransitionTo):
public void Ship()
{
    Status.TransitionTo(OrderStatus.Shipped); // throws if invalid, with full diagnostic message
    Status = OrderStatus.Shipped;
}
```

### Pattern 5: Query Handler Migration — Enum.TryParse Replacement

**What:** Current query handlers use `Enum.TryParse<OrderStatus>()` to parse status filter strings. With SmartEnum, replace with `SmartEnum.TryFromName()`.
**Example:**
```csharp
// Before:
if (!string.IsNullOrWhiteSpace(request.Status)
    && Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out OrderStatus statusFilter))

// After:
if (!string.IsNullOrWhiteSpace(request.Status)
    && OrderStatus.TryFromName(request.Status, ignoreCase: true, out OrderStatus? statusFilter))
```
This affects: `GetAllOrdersQueryHandler`, `GetOrdersByBuyerQueryHandler`, `GetProductsQueryHandler`.

### Pattern 6: Array Comparisons in LINQ — SmartEnum and Contains()

**What:** `GetOrderDashboardQueryHandler` uses `ExcludedStatuses.Contains(o.Status)` and `PendingStatuses.Contains(o.Status)` with static `OrderStatus[]` arrays. SmartEnum equality is reference-based for same-value instances (singleton pattern), so `Contains()` works for in-memory evaluation.
**EF Core concern:** EF Core must be able to translate `o.Status == someSmartEnum` to SQL. Since the EF converter stores strings, EF Core will translate comparisons to string comparisons if the ValueConverter is configured correctly.
**Recommendation:** Test that the static array `Contains()` pattern works with EF Core translation after migration. If EF Core cannot translate SmartEnum comparison, use `.Where(o => ExcludedStatuses.Select(s => s.Name).Contains(o.Status.Name))` as a fallback.

### Anti-Patterns to Avoid

- **Using `ConfigureSmartEnum()` without verifying storage type:** The EF Core convention package's `ConfigureSmartEnum()` stores by integer Value. The current DB schema stores strings. Using it would silently break existing data.
- **Sealing the SmartEnum types:** The CONTEXT.md decision is that SmartEnum types must be open (not sealed), allowing potential subclassing.
- **Adding `[JsonConverter]` per-property instead of globally:** With `OrderStatus` typed directly in DTOs, property-level attributes require modifying all DTO records. Global registration in `ConfigureHttpJsonOptions` is cleaner.
- **ToString() in LINQ projections:** `GetProductsQueryHandler` has `p.Status.ToString()` in an EF Core `.Select()` projection. After SmartEnum migration, `SmartEnum.ToString()` returns the Name by default, so this works in-memory. However, EF Core may not translate `ToString()` to SQL. Since this is a `Select()` projection (not a `Where()` filter), it executes in memory after materialization — verify this is acceptable, or map to `.Name` explicitly.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| State enumeration base class | Custom abstract class with Name/Value/List/FromName/FromValue | `Ardalis.SmartEnum` | Handles thread-safe singleton instances, equality, collection of all members, TryFrom methods, guard clause integration |
| EF Core value converter | Manual ValueConverter<TEnum,string> with reflection | Custom `SmartEnumStringConverter<TEnum>` wrapping SmartEnum.FromName | SmartEnum.FromName is already optimized; just wrap it |
| JSON serialization | Custom JsonConverter with GetTypeInfo/Read/Write | `SmartEnumNameConverter<TEnum,TValue>` from Ardalis.SmartEnum.SystemTextJson | Already handles null, type compatibility, and name lookup |
| State transition validation | Per-method if/throw guards | `TransitionTo()` on SmartEnum base with `CanTransitionTo()` abstract method | Centralizes transition rules in one place; exception message auto-includes valid states |

**Key insight:** The transition validation pattern in SmartEnum (abstract `CanTransitionTo()` + `TransitionTo()`) eliminates the scattered `if (Status != X) throw` guards currently spread across 6+ Order methods and 3 Product methods.

---

## Common Pitfalls

### Pitfall 1: ConfigureSmartEnum() Stores by Int Value, Not String Name

**What goes wrong:** Developer adds `Ardalis.SmartEnum.EFCore` and calls `configurationBuilder.ConfigureSmartEnum()`. This configures all SmartEnum properties to store their integer `Value`. The existing database schema stores strings ("Submitted", "Paid", etc.). Queries silently fail or EF Core throws conversion errors because the DB has strings but the converter expects ints.
**Why it happens:** The EF Core package's default converter is `SmartEnumConverter<TEnum, TValue>` which maps `item.Value` to the DB — correct for int-stored schemas, wrong for string-stored schemas.
**How to avoid:** Do NOT add `Ardalis.SmartEnum.EFCore` package. Write a custom `SmartEnumStringConverter<TEnum>` that maps `item.Name` ↔ `string`. Register per-type in `BaseDbContext.ConfigureConventions()`.
**Warning signs:** Migration snapshot still shows `b.Property<string>("Status")` but a new empty migration is generated — this means EF detected a type change.

### Pitfall 2: SmartEnum Serializes as JSON Object Without Converter

**What goes wrong:** SmartEnum is a class with properties (Name, Value, etc.). Without a JsonConverter, `System.Text.Json` serializes `OrderStatus.Submitted` as `{"name":"Submitted","value":0,...}`. The frontend currently receives `"status": "Submitted"` and all status comparisons break.
**Why it happens:** System.Text.Json has no built-in awareness of SmartEnum.
**How to avoid:** Register `SmartEnumNameConverter<TEnum, int>` globally via `builder.Services.ConfigureHttpJsonOptions()`.
**Warning signs:** Frontend status stepper shows nothing (no case matches `{"name":"Submitted",...}`).

### Pitfall 3: LINQ EF Core Translation of SmartEnum Comparisons

**What goes wrong:** EF Core cannot translate `o.Status == OrderStatus.Submitted` if the ValueConverter is not registered, or if there is a mismatch between the registered converter type and the property type.
**Why it happens:** EF Core needs to know how to translate the comparison. With a properly registered ValueConverter, it should work. Without it (or with a misconfigured one), EF throws a translation exception at runtime.
**How to avoid:** Verify the `configurationBuilder.Properties<OrderStatus>().HaveConversion<SmartEnumStringConverter<OrderStatus>>()` registration is in `BaseDbContext.ConfigureConventions()`, which runs before model building.
**Warning signs:** `InvalidOperationException: The LINQ expression ... could not be translated` at runtime.

### Pitfall 4: OrderStatus Array Comparisons in Dashboard Query

**What goes wrong:** `GetOrderDashboardQueryHandler` uses static `OrderStatus[]` arrays with `.Contains()` in EF Core LINQ queries. This pattern works for plain enums (EF Core knows how to generate SQL `IN` clauses). For SmartEnum, EF Core needs the ValueConverter to handle array comparisons.
**Why it happens:** EF Core's SmartEnum ValueConverter handles single-value comparisons but array `.Contains()` with SmartEnum objects may or may not be translatable depending on EF Core version and provider.
**How to avoid:** Test the query after migration. If it fails, rewrite as `.Where(o => new[] { "Failed", "Cancelled" }.Contains(o.Status.Name))` — this works because EF Core can always translate string array Contains.
**Warning signs:** `NotSupportedException` or `InvalidOperationException` mentioning `Contains` in LINQ translation.

### Pitfall 5: OrderFailedConsumer Idempotency Check

**What goes wrong:** `OrderFailedConsumer` checks `if (order.Status == OrderStatus.Failed)`. With SmartEnum, this equality check works only if SmartEnum uses singleton equality (which Ardalis.SmartEnum does — same-value instances are reference-equal via static readonly fields).
**Why it happens:** SmartEnum equality is implemented via `Value.Equals()` comparison, not reference equality. The static readonly fields are singletons, so `order.Status == OrderStatus.Failed` compares the loaded object's status against the static singleton.
**How to avoid:** No change needed — SmartEnum equality works via `Equals()` comparison on the Value property. The EF Core ValueConverter loads the string and converts it back to the singleton instance via `FromName()`, so loaded entities have reference-equal instances to the static fields.
**Warning signs:** If equality checks fail after migration, check that the ValueConverter calls `FromName()` (which returns the singleton), not `new OrderStatus(...)` (which would not be reference-equal).

---

## Code Examples

Verified patterns from official sources:

### Complete OrderStatus SmartEnum
```csharp
// Source: Ardalis SmartEnum README (Context7: /ardalis/smartenum) + project decisions from CONTEXT.md
public abstract class OrderStatus : SmartEnum<OrderStatus>
{
    // Happy path
    public static readonly OrderStatus Submitted     = new SubmittedStatus();
    public static readonly OrderStatus StockReserved = new StockReservedStatus();
    public static readonly OrderStatus Paid          = new PaidStatus();
    public static readonly OrderStatus Confirmed     = new ConfirmedStatus();
    public static readonly OrderStatus Shipped       = new ShippedStatus();
    public static readonly OrderStatus Delivered     = new DeliveredStatus();
    // Terminal non-happy
    public static readonly OrderStatus Failed        = new FailedStatus();
    public static readonly OrderStatus Cancelled     = new CancelledStatus();

    private OrderStatus(string name, int value) : base(name, value) { }

    public abstract bool CanTransitionTo(OrderStatus next);

    public void TransitionTo(OrderStatus next)
    {
        if (!CanTransitionTo(next))
        {
            IEnumerable<string> validNames = List.Where(s => CanTransitionTo(s)).Select(s => s.Name);
            throw new InvalidOperationException(
                $"Cannot transition from '{Name}' to '{next.Name}'. " +
                $"Valid transitions from '{Name}': {string.Join(", ", validNames)}.");
        }
    }

    private sealed class SubmittedStatus : OrderStatus
    {
        public SubmittedStatus() : base("Submitted", 0) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == StockReserved || next == Paid || next == Failed || next == Cancelled;
    }

    private sealed class StockReservedStatus : OrderStatus
    {
        public StockReservedStatus() : base("StockReserved", 1) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == Paid || next == Failed || next == Cancelled;
    }

    private sealed class PaidStatus : OrderStatus
    {
        public PaidStatus() : base("Paid", 2) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == Confirmed || next == Cancelled;
    }

    private sealed class ConfirmedStatus : OrderStatus
    {
        public ConfirmedStatus() : base("Confirmed", 3) { }
        public override bool CanTransitionTo(OrderStatus next) => next == Shipped;
    }

    private sealed class ShippedStatus : OrderStatus
    {
        public ShippedStatus() : base("Shipped", 4) { }
        public override bool CanTransitionTo(OrderStatus next) => next == Delivered;
    }

    private sealed class DeliveredStatus : OrderStatus
    {
        public DeliveredStatus() : base("Delivered", 5) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }

    private sealed class FailedStatus : OrderStatus
    {
        public FailedStatus() : base("Failed", 6) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }

    private sealed class CancelledStatus : OrderStatus
    {
        public CancelledStatus() : base("Cancelled", 7) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }
}
```

### Complete ProductStatus SmartEnum
```csharp
// Source: Project CONTEXT.md decisions + Ardalis SmartEnum abstract behavior pattern
public abstract class ProductStatus : SmartEnum<ProductStatus>
{
    public static readonly ProductStatus Draft     = new DraftStatus();
    public static readonly ProductStatus Published = new PublishedStatus();
    public static readonly ProductStatus Archived  = new ArchivedStatus();

    private ProductStatus(string name, int value) : base(name, value) { }

    public abstract bool CanTransitionTo(ProductStatus next);

    public void TransitionTo(ProductStatus next)
    {
        if (!CanTransitionTo(next))
        {
            IEnumerable<string> validNames = List.Where(s => CanTransitionTo(s)).Select(s => s.Name);
            throw new InvalidOperationException(
                $"Cannot transition from '{Name}' to '{next.Name}'. " +
                $"Valid transitions from '{Name}': {string.Join(", ", validNames)}.");
        }
    }

    private sealed class DraftStatus : ProductStatus
    {
        public DraftStatus() : base("Draft", 0) { }
        public override bool CanTransitionTo(ProductStatus next) =>
            next == Published || next == Archived;
    }

    private sealed class PublishedStatus : ProductStatus
    {
        public PublishedStatus() : base("Published", 1) { }
        public override bool CanTransitionTo(ProductStatus next) =>
            next == Draft || next == Archived;
    }

    private sealed class ArchivedStatus : ProductStatus
    {
        public ArchivedStatus() : base("Archived", 2) { }
        public override bool CanTransitionTo(ProductStatus next) => false; // terminal
    }
}
```

### Generic EF Core String Converter
```csharp
// Location: src/BuildingBlocks/BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs
using Ardalis.SmartEnum;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroCommerce.BuildingBlocks.Common.Converters;

public class SmartEnumStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : SmartEnum<TEnum, int>
{
    public SmartEnumStringConverter()
        : base(
            smartEnum => smartEnum.Name,
            name => SmartEnum<TEnum, int>.FromName(name))
    { }
}
```

### BaseDbContext Convention Registration
```csharp
// BaseDbContext.cs — add SmartEnum converter registrations alongside existing ones
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    base.ConfigureConventions(configurationBuilder);

    configurationBuilder.RegisterAllInVogenEfCoreConverters();

    // SmartEnum string converters — stores by Name (compatible with existing string DB schema)
    configurationBuilder.Properties<OrderStatus>()
        .HaveConversion<SmartEnumStringConverter<OrderStatus>>();
    configurationBuilder.Properties<ProductStatus>()
        .HaveConversion<SmartEnumStringConverter<ProductStatus>>();

    configurationBuilder.Conventions.Add(_ => new Conventions.AuditableConvention());
    configurationBuilder.Conventions.Add(_ => new Conventions.ConcurrencyTokenConvention());
    configurationBuilder.Conventions.Add(_ => new Conventions.SoftDeletableConvention());
}
```

### JSON Global Registration
```csharp
// Program.cs — add before builder.Build()
// Required so SmartEnum serializes as "Submitted" not {"name":"Submitted","value":0}
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new SmartEnumNameConverter<OrderStatus, int>());
    options.SerializerOptions.Converters.Add(
        new SmartEnumNameConverter<ProductStatus, int>());
});
```

### Entity Configuration — Remove Explicit HasConversion
After BaseDbContext convention handles it, remove `HasConversion<string>()` from explicit entity configurations:
```csharp
// OrderConfiguration.cs — REMOVE this line (convention handles it now):
builder.Property(o => o.Status)
    .HasConversion<string>()  // <-- remove
    .HasMaxLength(32)
    .IsRequired();

// Keep MaxLength and IsRequired, they remain valid:
builder.Property(o => o.Status)
    .HasMaxLength(32)
    .IsRequired();
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Plain C# enum with scattered if/throw guards | SmartEnum abstract class with CanTransitionTo() per state | Phase 18 | Transition rules colocated with state definitions |
| `Enum.TryParse<T>()` | `SmartEnum.TryFromName()` | Phase 18 | More explicit, works with SmartEnum not plain enum |
| `HasConversion<string>()` in entity config | `configurationBuilder.Properties<T>().HaveConversion<SmartEnumStringConverter<T>>()` | Phase 18 | Convention-based, no per-entity config needed |
| `p.Status.ToString()` in LINQ | `p.Status.Name` (explicit) | Phase 18 | SmartEnum.Name is the string representation |

**Still current:**
- Frontend receives string values ("Submitted", "Paid", etc.) — unchanged by SmartEnum migration
- DB schema unchanged — still stores varchar/text "Submitted", "Paid", etc.
- No EF migrations needed (this is a stated constraint: "database storage details are not a concern")

---

## Open Questions

1. **EF Core LINQ translation of SmartEnum array Contains()**
   - What we know: `GetOrderDashboardQueryHandler` uses `ExcludedStatuses.Contains(o.Status)` where `ExcludedStatuses` is `OrderStatus[]`
   - What's unclear: Whether EF Core + Npgsql can translate `.Contains()` on a SmartEnum array to a SQL `IN` clause via the ValueConverter
   - Recommendation: After migration, run the dashboard query and check for translation exceptions. Fallback: replace with `new[] { "Failed", "Cancelled" }.Contains(o.Status.Name)` which is always translatable

2. **Product.Publish/Unpublish early-return guards**
   - What we know: `Product.Publish()` has `if (Status == ProductStatus.Published) return;` (idempotent no-op). With TransitionTo(), this would throw instead of silently returning.
   - What's unclear: Whether the caller expects idempotent behavior (no error when publishing an already-published product) vs. strict enforcement
   - Recommendation: The CONTEXT.md decisions say TransitionTo() throws on invalid transitions. If `Published → Published` is not a valid transition (it is not in the defined rules), TransitionTo() will throw. The entity methods should check before calling TransitionTo(), or handlers can guard. This is a Claude's Discretion item.

3. **`OrderStatus` type in `OrderSummaryDto`**
   - What we know: `OrderSummaryDto.Status` is typed as `OrderStatus` (not `string`). The JSON converter will serialize it correctly.
   - What's unclear: The `OrderSummaryDto` is used in LINQ `Select()` projection from `GetOrdersByBuyerQueryHandler` and `GetAllOrdersQueryHandler` — EF Core materializes these into `OrderStatus` objects using the ValueConverter.
   - Recommendation: No change needed to the DTO type. The EF Core ValueConverter handles loading; the JSON converter handles serialization.

---

## Codebase Impact Summary

**Files requiring changes:**

| File | Change Type | Notes |
|------|------------|-------|
| `Features/Ordering/Domain/ValueObjects/OrderStatus.cs` | Replace | Plain enum → abstract SmartEnum class |
| `Features/Catalog/Domain/ValueObjects/ProductStatus.cs` | Replace | Plain enum → abstract SmartEnum class |
| `BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs` | New | Generic EF Core ValueConverter storing by Name |
| `Common/Persistence/BaseDbContext.cs` | Modify | Add SmartEnum converter registrations in ConfigureConventions |
| `Program.cs` | Modify | Register SmartEnumNameConverter in ConfigureHttpJsonOptions |
| `Features/Ordering/Domain/Entities/Order.cs` | Modify | Replace if/throw guards with TransitionTo() calls |
| `Features/Catalog/Domain/Entities/Product.cs` | Modify | Replace equality guards with TransitionTo() calls |
| `Features/Ordering/Infrastructure/Configurations/OrderConfiguration.cs` | Modify | Remove `HasConversion<string>()` (convention handles it) |
| `Features/Catalog/Infrastructure/Configurations/ProductConfiguration.cs` | Modify | Remove `HasConversion<string>()` (convention handles it) |
| `Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs` | Modify | Replace `Enum.TryParse<OrderStatus>()` with `OrderStatus.TryFromName()` |
| `Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQueryHandler.cs` | Modify | Replace `Enum.TryParse<OrderStatus>()` with `OrderStatus.TryFromName()` |
| `Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs` | Modify | Replace `Enum.TryParse<ProductStatus>()` with `ProductStatus.TryFromName()` |
| `Features/Ordering/Application/Queries/GetOrderDashboard/GetOrderDashboardQueryHandler.cs` | Verify | Test Contains() translation; may need string array fallback |
| `Features/Catalog/Domain/Events/ProductStatusChangedDomainEvent.cs` | Verify | `newStatus.ToString()` — works if SmartEnum.ToString() returns Name |
| `BuildingBlocks.Common/BuildingBlocks.Common.csproj` | Modify | Add `Ardalis.SmartEnum` package reference |
| `MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` | Modify | Add `Ardalis.SmartEnum.SystemTextJson` package reference |

**Files NOT requiring changes:**
- All EF Core migrations (no schema change)
- `OrderDto.cs`, `OrderSummaryDto.cs` (Status type stays as SmartEnum; JSON converter handles serialization)
- Frontend TypeScript — receives same string values

---

## Validation Architecture

> `workflow.nyquist_validation` is not present in `.planning/config.json` — section skipped.

---

## Sources

### Primary (HIGH confidence)
- `/ardalis/smartenum` (Context7) — State machine pattern, ValueConverter, SmartEnumNameConverter usage, ConfigureSmartEnum, package list
- `https://github.com/ardalis/SmartEnum/blob/main/src/SmartEnum.EFCore/SmartEnumConverter.cs` — Confirmed SmartEnumConverter stores by Value (int), not Name

### Secondary (MEDIUM confidence)
- `https://www.nuget.org/packages/Ardalis.SmartEnum/` — Version 8.2.0 confirmed (WebSearch verified)
- `https://www.nuget.org/packages/Ardalis.SmartEnum.SystemTextJson` — Version 8.1.0 confirmed; SmartEnumNameConverter and SmartEnumValueConverter confirmed (WebSearch + NuGet)
- `https://blog.nimblepros.com/blogs/persisting-a-smart-enum-with-entity-framework-core/` — Confirms HasConversion pattern with FromName/FromValue; official Ardalis blog

### Tertiary (LOW confidence)
- None needed — all critical claims verified at HIGH or MEDIUM level

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — version numbers verified from NuGet; package names confirmed from Context7 and official docs
- Architecture: HIGH — SmartEnum pattern confirmed from official README (Context7); EF converter behavior verified from source code; JSON serialization confirmed
- Pitfalls: HIGH — ConfigureSmartEnum int-vs-string issue confirmed directly from SmartEnumConverter source code; DB schema confirmed from migration snapshots; LINQ concerns are well-known EF Core behavior

**Research date:** 2026-02-24
**Valid until:** 2026-05-24 (90 days — stable library)
