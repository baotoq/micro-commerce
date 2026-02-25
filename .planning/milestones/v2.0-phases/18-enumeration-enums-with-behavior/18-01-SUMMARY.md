---
phase: 18-enumeration-enums-with-behavior
plan: "01"
subsystem: domain-model
tags: [smartenum, ef-core, json, value-objects, state-machine]
dependency_graph:
  requires: []
  provides: [PRIM-02]
  affects: [ordering, catalog, persistence, serialization]
tech_stack:
  added:
    - Ardalis.SmartEnum 8.2.0 (BuildingBlocks.Common)
    - Ardalis.SmartEnum.SystemTextJson 8.1.0 (MicroCommerce.ApiService)
    - Microsoft.EntityFrameworkCore 10.0.0 (BuildingBlocks.Common — for ValueConverter base class)
  patterns:
    - SmartEnum abstract class with sealed inner classes per state
    - Generic EF Core ValueConverter storing SmartEnum by Name
    - Global JSON converter via ConfigureHttpJsonOptions
key_files:
  created:
    - src/BuildingBlocks/BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs
  modified:
    - src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderStatus.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Domain/ValueObjects/ProductStatus.cs
    - src/MicroCommerce.ApiService/Common/Persistence/BaseDbContext.cs
    - src/MicroCommerce.ApiService/Program.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Configurations/OrderConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Configurations/ProductConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/Product.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs
decisions:
  - "SmartEnum types are abstract (not sealed) to allow subclassing per CONTEXT.md"
  - "SmartEnumStringConverter stores by Name (not Value) to preserve string-based DB schema"
  - "Per-type HaveConversion registration in BaseDbContext (not IModelFinalizingConvention) for simplicity with 2 types"
  - "HasConversion<string>() removed from OrderConfiguration and ProductConfiguration — convention-based approach cleaner"
  - "Enum.TryParse replaced with SmartEnum.TryFromName in all 3 query handlers as part of blocking fix"
metrics:
  duration: 3 min
  completed_date: 2026-02-24
  tasks_completed: 2
  files_modified: 13
  files_created: 1
---

# Phase 18 Plan 01: SmartEnum Infrastructure Summary

SmartEnum infrastructure (PRIM-02) fully in place: Ardalis.SmartEnum with generic EF Core string converter and JSON serialization for OrderStatus (8 states) and ProductStatus (3 states) with state transition rules.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Install packages and create SmartEnum types with transition rules | f248e92b | 9 files |
| 2 | Create generic EF Core converter and register EF Core + JSON converters | 012c68ff | 6 files |

## What Was Built

### SmartEnum Types

**OrderStatus** (`Features/Ordering/Domain/ValueObjects/OrderStatus.cs`):
- Abstract class inheriting `SmartEnum<OrderStatus>` with 8 sealed inner state classes
- Backing values: Submitted=0, StockReserved=1, Paid=2, Confirmed=3, Shipped=4, Delivered=5, Failed=6, Cancelled=7
- State machine: Submitted → {StockReserved, Paid, Failed, Cancelled}; StockReserved → {Paid, Failed, Cancelled}; Paid → {Confirmed, Cancelled}; Confirmed → Shipped; Shipped → Delivered; Delivered/Failed/Cancelled = terminal
- `CanTransitionTo(OrderStatus next)` abstract per state, `TransitionTo(OrderStatus next)` throws `InvalidOperationException` on invalid transitions with full diagnostic message

**ProductStatus** (`Features/Catalog/Domain/ValueObjects/ProductStatus.cs`):
- Abstract class inheriting `SmartEnum<ProductStatus>` with 3 sealed inner state classes
- Backing values: Draft=0, Published=1, Archived=2
- State machine: Draft → {Published, Archived}; Published → {Draft, Archived}; Archived = terminal
- Same `CanTransitionTo`/`TransitionTo` pattern

### SmartEnumStringConverter

`BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs`:
- Generic `ValueConverter<TEnum, string>` where `TEnum : SmartEnum<TEnum, int>`
- Stores by `Name` (string) not `Value` (int) — critical for compatibility with existing PostgreSQL string schema
- Uses `SmartEnum<TEnum, int>.FromName(name)` for deserialization (returns singleton instance)

### EF Core Registration

`BaseDbContext.ConfigureConventions()` now registers:
```csharp
configurationBuilder.Properties<OrderStatus>()
    .HaveConversion<SmartEnumStringConverter<OrderStatus>>();
configurationBuilder.Properties<ProductStatus>()
    .HaveConversion<SmartEnumStringConverter<ProductStatus>>();
```
Positioned after `RegisterAllInVogenEfCoreConverters()` and before convention Add calls.

### JSON Registration

`Program.cs` now registers:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new SmartEnumNameConverter<OrderStatus, int>());
    options.SerializerOptions.Converters.Add(new SmartEnumNameConverter<ProductStatus, int>());
});
```
Ensures `"status": "Submitted"` not `"status": {"name":"Submitted","value":0}` in all API responses.

## Deviations from Plan

### Auto-fixed Issues (Rule 3 — Blocking Issues)

**1. [Rule 3 - Blocking] Fixed C# pattern matching incompatibility in Order.MarkAsPaid/MarkAsFailed**
- **Found during:** Task 1
- **Issue:** Order.cs used `is not (X or Y)` C# pattern matching syntax which requires constant values; SmartEnum is a reference type, not a constant
- **Fix:** Replaced with standard equality comparisons (`Status != X && Status != Y`)
- **Files modified:** `Features/Ordering/Domain/Entities/Order.cs`
- **Commit:** f248e92b

**2. [Rule 3 - Blocking] Initialized Status in EF Core private constructors**
- **Found during:** Task 1
- **Issue:** CS8618 — `Status` property (now a reference type `OrderStatus`/`ProductStatus`) must be initialized in constructor; EF Core private constructors didn't set it
- **Fix:** Added `Status = OrderStatus.Submitted` in `Order(OrderId id)` and `Status = ProductStatus.Draft` in `Product(ProductId id)` constructors
- **Files modified:** `Features/Ordering/Domain/Entities/Order.cs`, `Features/Catalog/Domain/Entities/Product.cs`
- **Commit:** f248e92b

**3. [Rule 3 - Blocking] Replaced Enum.TryParse with SmartEnum.TryFromName in query handlers**
- **Found during:** Task 1
- **Issue:** CS0453 — `Enum.TryParse<T>()` requires a non-nullable value type; SmartEnum is a reference type
- **Fix:** Replaced with `OrderStatus.TryFromName(request.Status, ignoreCase: true, out OrderStatus? statusFilter)` in 3 query handlers
- **Files modified:** `GetAllOrdersQueryHandler.cs`, `GetOrdersByBuyerQueryHandler.cs`, `GetProductsQueryHandler.cs`
- **Commit:** f248e92b

**4. [Rule 3 - Implicit] Removed HasConversion<string>() from entity configurations**
- **Found during:** Task 2
- **Issue:** With the base convention registering `SmartEnumStringConverter`, the explicit `HasConversion<string>()` in entity configurations would conflict (convention-based vs explicit)
- **Fix:** Removed `HasConversion<string>()` from `OrderConfiguration.cs` and `ProductConfiguration.cs`; `HasMaxLength` and `IsRequired` retained
- **Files modified:** `OrderConfiguration.cs`, `ProductConfiguration.cs`
- **Commit:** 012c68ff

## Self-Check: PASSED

Files created:
- FOUND: src/BuildingBlocks/BuildingBlocks.Common/Converters/SmartEnumStringConverter.cs

Commits:
- FOUND: f248e92b (Task 1)
- FOUND: 012c68ff (Task 2)

Build: 0 errors, 0 C# warnings (2 pre-existing NuGet vulnerability warnings for SixLabors.ImageSharp are out of scope)
