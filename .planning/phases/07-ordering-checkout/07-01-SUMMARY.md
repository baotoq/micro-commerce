# Phase 7 Plan 01: Order Domain Model & Persistence Summary

**One-liner:** Order aggregate root with DDD value objects, domain events, EF Core configurations in ordering schema with InitialOrdering migration

## Execution Details

| Field | Value |
|-------|-------|
| Phase | 07-ordering-checkout |
| Plan | 01 |
| Duration | ~2 minutes |
| Completed | 2026-02-10 |
| Tasks | 2/2 |

## What Was Built

### Task 1: Order domain model, value objects, and domain events
**Commit:** `237a46a0`

Created the complete Order domain model following established patterns from Cart and Inventory modules:

- **OrderId, OrderItemId** -- StronglyTypedId<Guid> with New() and From() factories
- **OrderNumber** -- Generates MC-XXXXXX format using cryptographically random chars (excludes ambiguous 0/O/1/I/L)
- **OrderStatus** -- Enum: Submitted, StockReserved, Paid, Confirmed, Failed, Cancelled
- **ShippingAddress** -- Sealed record value object with Name, Email, Street, City, State, ZipCode (all validated non-empty)
- **OrderItem** -- Entity with ProductId, ProductName, UnitPrice, ImageUrl, Quantity, LineTotal (computed)
- **Order** -- Aggregate root extending BaseAggregateRoot<OrderId> with:
  - Create() factory: auto-calculates Subtotal + $5.99 shipping + 8% tax, raises OrderSubmittedDomainEvent
  - MarkAsPaid(): transitions to Paid, sets PaidAt, raises OrderPaidDomainEvent
  - MarkAsFailed(reason): transitions to Failed, raises OrderFailedDomainEvent
  - Confirm(): transitions to Confirmed (saga downstream)
  - MarkStockReserved(): transitions to StockReserved (saga internal)
- **Domain events:** OrderSubmittedDomainEvent, OrderPaidDomainEvent, OrderFailedDomainEvent (thin ID-only pattern)

### Task 2: EF Core configurations, DbContext update, and migration
**Commit:** `cadb1fbc`

- **OrderConfiguration** -- OwnsOne for ShippingAddress, OrderNumber with unique index, BuyerId index, CreatedAt descending index, xmin concurrency, Status stored as string
- **OrderItemConfiguration** -- OrderItemId/OrderId conversions, precision(18,2) for money fields, cascade delete FK
- **OrderingDbContext** -- Updated with Order and OrderItem DbSets
- **InitialOrdering migration** -- Creates ordering.Orders and ordering.OrderItems tables with all columns, indexes, and constraints

## Key Files

### Created
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/OrderItem.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderId.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderItemId.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderNumber.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderStatus.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/ShippingAddress.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Events/OrderSubmittedDomainEvent.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Events/OrderPaidDomainEvent.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Events/OrderFailedDomainEvent.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Configurations/OrderConfiguration.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Configurations/OrderItemConfiguration.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/20260210030603_InitialOrdering.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/20260210030603_InitialOrdering.Designer.cs`
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/OrderingDbContextModelSnapshot.cs`

### Modified
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/OrderingDbContext.cs`

### Removed
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/.gitkeep`
- `src/MicroCommerce.ApiService/Features/Ordering/Application/.gitkeep`

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| OwnsOne for ShippingAddress mapping | Clearer column naming (ShippingAddress_Name etc.) vs ComplexProperty |
| RandomNumberGenerator for OrderNumber | Cryptographically secure randomness for order codes |
| Status transitions with guard clauses | Prevent invalid state transitions (e.g., can't confirm unpaid order) |
| Flat $5.99 shipping + 8% tax | Simple calculation, configurable later via settings |
| Raw Guid for ProductId in OrderItem | Cross-module boundary pattern consistent with Inventory module |

## Deviations from Plan

None -- plan executed exactly as written.

## Verification

- [x] `dotnet build src/MicroCommerce.ApiService/` -- zero errors
- [x] Migration file exists at `Features/Ordering/Infrastructure/Migrations/20260210030603_InitialOrdering.cs`
- [x] Order.Create() produces orders with OrderNumber (MC-XXXXXX), calculated totals, OrderSubmittedDomainEvent
- [x] All value objects have correct factories/conversions
- [x] EF migration creates ordering.Orders and ordering.OrderItems with proper columns and indexes

## Next Phase Readiness

The Order domain model is ready for:
- **Plan 02**: Checkout CQRS commands/queries (SubmitOrder, GetOrder)
- **Plan 03**: Ordering saga with MassTransit state machine
- **Plan 04**: MassTransit outbox integration for OrderingDbContext
