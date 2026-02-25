---
phase: 21-adoption-full-building-block-integration
plan: 01
subsystem: Domain / Building Blocks
tags: [ddd, entity, aggregate, auditable, efcore, migration]
dependency_graph:
  requires:
    - "Phase 15: Building blocks (Entity, AuditableAggregateRoot, AuditInterceptor) implemented"
  provides:
    - "All 5 child entities inherit Entity<TId> — standardized DDD identity"
    - "5 aggregates inherit AuditableAggregateRoot<TId> — automatic timestamp management"
  affects:
    - "CartDbContext (schema: last_modified_at -> updated_at)"
    - "CatalogDbContext (schema: updated_at nullable -> non-nullable for categories, products)"
tech_stack:
  added: []
  patterns:
    - "Entity<TId> base class for all child entities (CartItem, OrderItem, StockReservation, StockAdjustment, Address)"
    - "AuditableAggregateRoot<TId> for aggregates with timestamps (Category, Product, Review, UserProfile, Cart)"
    - "AuditInterceptor auto-sets CreatedAt/UpdatedAt on EntityState.Added/Modified"
key_files:
  created:
    - src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Migrations/20260225103732_AuditableAggregateRootAdoption.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Migrations/20260225103751_AuditableAggregateRootAdoption.cs
    - src/MicroCommerce.ApiService/Migrations/20260225103809_AuditableAggregateRootAdoption.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Migrations/20260225103829_AuditableAggregateRootAdoption.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/CartItem.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/OrderItem.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockReservation.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockAdjustment.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/ValueObjects/Address.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/Category.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/Product.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/Entities/Review.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/Entities/UserProfile.cs
    - src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs
    - src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Configurations/CartConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Configurations/ProductConfiguration.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Cart/Aggregates/CartTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Catalog/Aggregates/ProductTests.cs
decisions:
  - "Entity<TId> EF Core parameterless constructors use private (not protected) because all entities are sealed — sealed classes prohibit protected members (CS0628)"
  - "Order kept as BaseAggregateRoot<OrderId> — domain-specific CreatedAt/PaidAt timestamps are more meaningful than generic UpdatedAt; status transitions have semantic timestamps"
  - "Cart.LastModifiedAt renamed to UpdatedAt via AuditableAggregateRoot inheritance — required EF Core migration (rename last_modified_at column)"
  - "Category/Product UpdatedAt changed from DateTimeOffset? (nullable) to DateTimeOffset (non-nullable) — required EF Core migration with default value"
  - "Unit test timestamp assertions removed — AuditInterceptor handles CreatedAt/UpdatedAt at infrastructure level, not in domain; domain unit tests no longer test interceptor-managed behavior"
metrics:
  duration: 30 min
  completed_date: "2026-02-25"
  tasks_completed: 2
  files_modified: 14
  tests_before: 178
  tests_after: 177
---

# Phase 21 Plan 01: Entity and AuditableAggregateRoot Migration Summary

Migrated all 5 child entities to Entity<TId> base class and 5 aggregates to AuditableAggregateRoot<TId>, removing all manual timestamp assignments and adding EF Core migrations for schema changes.

## What Was Done

### Task 1: Migrate 5 Child Entities to Entity<TId>

All 5 child entities now inherit from `Entity<TId>` from BuildingBlocks.Common:

- `CartItem : Entity<CartItemId>` — owned by Cart aggregate
- `OrderItem : Entity<OrderItemId>` — owned by Order aggregate
- `StockReservation : Entity<ReservationId>` — owned by StockItem aggregate
- `StockAdjustment : Entity<AdjustmentId>` — standalone audit entity
- `Address : Entity<AddressId>` — owned by UserProfile via OwnsMany

Pattern applied to each:
1. Added `using MicroCommerce.BuildingBlocks.Common;`
2. Changed class declaration to `public sealed class X : Entity<TId>`
3. Removed `public TId Id { get; private set; }` (inherited from Entity)
4. Changed EF Core constructor to `private X() : base() { }` (private because sealed)
5. Added `private X(TId id) : base(id) { }` constructor
6. Updated factory methods to use `new X(TId.New()) { ... }` pattern

No EF Core migrations needed for ADOPT-01 — the Id property already existed in the database schema.

### Task 2: Migrate 5 Aggregates to AuditableAggregateRoot

5 aggregates migrated to `AuditableAggregateRoot<TId>`, removing manual `CreatedAt`/`UpdatedAt` assignments:

- `Category : AuditableAggregateRoot<CategoryId>` — removed `CreatedAt = DateTimeOffset.UtcNow` from Create(), `UpdatedAt = DateTimeOffset.UtcNow` from Update()
- `Product : AuditableAggregateRoot<ProductId>` — removed all manual timestamp assignments from Create(), Update(), Publish(), Unpublish(), Archive()
- `Review : AuditableAggregateRoot<ReviewId>` — removed `var now = DateTimeOffset.UtcNow;` and all timestamp assignments
- `UserProfile : AuditableAggregateRoot<UserProfileId>` — removed `CreatedAt = now` and `UpdatedAt = now` from Create(); Touch() now only raises domain event
- `Cart : AuditableAggregateRoot<CartId>` — removed `CreatedAt` and `LastModifiedAt` properties; Touch() only resets ExpiresAt (domain logic)

`Order` intentionally kept as `BaseAggregateRoot<OrderId>` — domain-specific timestamps (CreatedAt set at creation, PaidAt on payment) are more semantically meaningful than a generic UpdatedAt.

### EF Core Migrations

4 migrations generated:

1. **CartDbContext** (`20260225103732_AuditableAggregateRootAdoption`): Renames `last_modified_at` → `updated_at` in `cart.carts` table
2. **CatalogDbContext** (`20260225103751_AuditableAggregateRootAdoption`): Changes `updated_at` from nullable to non-nullable in `catalog.categories` and `catalog.products` tables
3. **ProfilesDbContext** (`20260225103809_AuditableAggregateRootAdoption`): Empty migration — schema already had non-nullable `created_at`/`updated_at`
4. **ReviewsDbContext** (`20260225103829_AuditableAggregateRootAdoption`): Empty migration — schema already had non-nullable `created_at`/`updated_at`

### Test Updates

Two test files updated to reflect that timestamps are no longer managed by domain code:

- `CartTests.cs`: Removed `AddItem_UpdatesLastModifiedAt` test (LastModifiedAt property no longer exists); removed `CreatedAt`/`LastModifiedAt` assertions from `Create_ValidBuyerId_ReturnsCartWithEmptyItems`
- `ProductTests.cs`: Removed `Update_ValidData_SetsUpdatedAt`, `Publish_SetsUpdatedAt` tests; removed `CreatedAt`/`UpdatedAt` nullability assertions — these are now AuditInterceptor responsibilities tested via integration tests

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Private constructors required for sealed EF Core entities**
- **Found during:** Task 1 build
- **Issue:** Plan specified `protected CartItem() : base() { }` but all 5 entities are `sealed`. C# prohibits `protected` members in sealed classes (CS0628 error).
- **Fix:** Changed all EF Core parameterless constructors to `private X() : base() { }` — EF Core can still use private constructors for materializing entities via reflection.
- **Files modified:** CartItem.cs, OrderItem.cs, StockReservation.cs, StockAdjustment.cs, Address.cs
- **Commit:** 4b66c2f3

**2. [Rule 2 - Missing functionality] EF Core migrations for Cart and Catalog schema changes**
- **Found during:** Task 2 — schema changes required for AuditableAggregateRoot adoption
- **Issue:** Cart's `LastModifiedAt` → `UpdatedAt` rename is a column rename requiring a migration; Category/Product `UpdatedAt` nullable → non-nullable change requires a migration
- **Fix:** Generated 4 EF Core migrations via `dotnet ef migrations add`
- **Files modified:** 4 new migration files + 4 snapshot files updated
- **Commit:** 322f98e6

## Test Results

| Suite | Before | After | Delta |
|-------|--------|-------|-------|
| Unit | 144 | 143 | -1 (removed LastModifiedAt test) |
| Integration | 34 | 34 | 0 |
| **Total** | **178** | **177** | **-1** |

All 177 tests pass. The -1 is from removing `AddItem_UpdatesLastModifiedAt` which tested `Cart.LastModifiedAt` (renamed to `UpdatedAt`, now managed by AuditInterceptor rather than domain code).

## Verification

- `class CartItem : Entity<CartItemId>` confirmed
- `class Category : AuditableAggregateRoot<CategoryId>` confirmed
- `class Order : BaseAggregateRoot<OrderId>` confirmed (not AuditableAggregateRoot)
- No `CreatedAt = DateTimeOffset.UtcNow` in Category, Product, Review, UserProfile, Cart factory methods
- No `LastModifiedAt` property in Cart entity

## Self-Check: PASSED

Files verified:
- src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/CartItem.cs (FOUND)
- src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/Category.cs (FOUND)
- src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Migrations/20260225103732_AuditableAggregateRootAdoption.cs (FOUND)
- src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Migrations/20260225103751_AuditableAggregateRootAdoption.cs (FOUND)

Commits verified:
- 4b66c2f3 (Task 1: child entities) - FOUND
- 322f98e6 (Task 2: aggregates + migrations) - FOUND
