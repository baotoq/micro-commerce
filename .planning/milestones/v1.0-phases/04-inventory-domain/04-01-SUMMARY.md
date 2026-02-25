---
phase: 04-inventory-domain
plan: 01
subsystem: inventory
tags: [ddd, aggregate, ef-core, postgresql, concurrency]
dependency-graph:
  requires: [01-02, 01-05]
  provides: [inventory-domain-model, inventory-dbcontext, inventory-migration]
  affects: [04-02, 04-03, 05-xx]
tech-stack:
  added: []
  patterns: [aggregate-root, strongly-typed-id, value-object, domain-events, xmin-concurrency, schema-isolation]
key-files:
  created:
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockItem.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockReservation.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockAdjustment.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/ValueObjects/StockItemId.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/ValueObjects/ReservationId.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/ValueObjects/AdjustmentId.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/ValueObjects/Quantity.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Events/StockAdjustedDomainEvent.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Events/StockReservedDomainEvent.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Events/StockReleasedDomainEvent.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Domain/Events/StockLowDomainEvent.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Configurations/StockItemConfiguration.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Configurations/StockReservationConfiguration.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Configurations/StockAdjustmentConfiguration.cs
    - code/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Migrations/20260207181100_InitialInventory.cs
  modified:
    - code/MicroCommerce.ApiService/Features/Inventory/Infrastructure/InventoryDbContext.cs
decisions:
  - id: inv-001
    decision: "Raw Guid for ProductId in Inventory module"
    rationale: "Cross-module boundary - Inventory does not reference Catalog types"
  - id: inv-002
    decision: "xmin concurrency token via IsRowVersion()"
    rationale: "PostgreSQL native optimistic concurrency without extra columns"
  - id: inv-003
    decision: "15-minute TTL for stock reservations"
    rationale: "Balance between holding stock for checkout and releasing abandoned carts"
  - id: inv-004
    decision: "StockLow threshold at 10 units"
    rationale: "Simple threshold for low stock alerts, can be made configurable later"
  - id: inv-005
    decision: "No domain event on StockItem creation"
    rationale: "Stock items are created by consumers (event handlers), not user action"
metrics:
  duration: "4 minutes"
  completed: 2026-02-07
---

# Phase 4 Plan 1: Inventory Domain Model Summary

**StockItem aggregate with reservation management, audit trail, and xmin optimistic concurrency on PostgreSQL inventory schema**

## What Was Built

### Task 1: Domain Entities, Value Objects, and Events

**StockItem aggregate** (`BaseAggregateRoot<StockItemId>`) with three core operations:
- `AdjustStock(adjustment, reason, adjustedBy)` - enforces non-negative invariant, raises `StockAdjustedDomainEvent` and conditionally `StockLowDomainEvent` when quantity drops to 10 or below
- `Reserve(quantity)` - validates positive quantity and sufficient available stock, creates TTL-based `StockReservation` (15 min), raises `StockReservedDomainEvent`
- `ReleaseReservation(reservationId)` - idempotent removal of reservation, raises `StockReleasedDomainEvent`

Computed `AvailableQuantity` property subtracts active (non-expired) reservation quantities from `QuantityOnHand`.

**Supporting entities:**
- `StockReservation` - owned by StockItem, has TTL-based expiration with `IsExpired` computed property
- `StockAdjustment` - standalone audit record capturing adjustment amount, resulting quantity, reason, and actor

**Value objects:** `StockItemId`, `ReservationId`, `AdjustmentId` (all `StronglyTypedId<Guid>`), `Quantity` (non-negative int wrapper via `ValueObject`)

**Domain events:** `StockAdjustedDomainEvent`, `StockReservedDomainEvent`, `StockReleasedDomainEvent`, `StockLowDomainEvent` - all thin records with Guid IDs only

### Task 2: DbContext, EF Configurations, and Migration

**InventoryDbContext** updated with three DbSets, `inventory` schema isolation, namespace-filtered configuration discovery.

**EF Configurations:**
- `StockItemConfiguration` - xmin concurrency via `IsRowVersion()`, unique ProductId index, cascade delete to reservations, field access mode for `_reservations` backing field
- `StockReservationConfiguration` - ExpiresAt index for cleanup queries
- `StockAdjustmentConfiguration` - StockItemId index for history, CreatedAt descending index for chronological queries

**Migration** `InitialInventory` creates StockItems, StockReservations, StockAdjustments tables in the `inventory` schema with all indexes and constraints.

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| Commit | Type | Description |
|--------|------|-------------|
| `00f0c76e` | feat | Domain entities, value objects, and events |
| `f7454fea` | feat | DbContext, EF configurations, and migration |

## Verification Results

- `dotnet build code/` compiles cleanly with 0 warnings, 0 errors
- Migration file exists at `Features/Inventory/Infrastructure/Migrations/20260207181100_InitialInventory.cs`
- Migration creates tables in `inventory` schema with xmin row version on StockItems
- All domain events extend `DomainEvent` base class
- StockItem.AdjustStock validates non-negative stock invariant
- StockItem.Reserve validates positive quantity and sufficient available stock

## Next Phase Readiness

Domain model is ready for:
- **04-02**: CQRS commands and queries (AdjustStock, Reserve, GetStockLevel)
- **04-03**: API endpoints exposing inventory operations
- **Phase 5**: Event consumers that create StockItems when products are created in Catalog
