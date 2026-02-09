# Phase 6 Plan 01: Cart Domain Model & Persistence Summary

**One-liner:** Cart aggregate with CartItem entity, strongly-typed IDs, 30-day TTL, max-99 quantity invariant, EF Core schema-isolated persistence, and cookie-based buyer identity.

---

## What Was Done

### Task 1: Cart domain model and value objects
- Created `CartId` and `CartItemId` strongly-typed IDs following `StockItemId` pattern
- Created `Cart` aggregate root extending `BaseAggregateRoot<CartId>` with:
  - Factory method `Create(Guid buyerId)` setting timestamps and 30-day TTL
  - `AddItem()` that merges existing products or creates new items, capped at 99
  - `UpdateItemQuantity()` with 1-99 range validation
  - `RemoveItem()` with idempotent removal
  - Private `Touch()` that resets `LastModifiedAt` and `ExpiresAt`
  - `[Timestamp] uint Version` for PostgreSQL xmin concurrency
- Created `CartItem` entity with `IncrementQuantity()` (caps at 99) and `SetQuantity()` (validates 1-99)
- Created `BuyerIdentity` static helper: checks `sub` claim, then `buyer_id` cookie, then generates new GUID with HttpOnly/Secure/Lax cookie (7-day MaxAge)

### Task 2: EF Core configurations, CartDbContext update, and migration
- Updated `CartDbContext` with `DbSet<Cart>` and `DbSet<CartItem>`
- Created `CartConfiguration` with value converters, indexes on `BuyerId` and `ExpiresAt`, cascade delete for items
- Created `CartItemConfiguration` with `ProductName` maxlength 200, `UnitPrice` precision 18,2
- Generated `InitialCart` migration creating `cart.Carts` and `cart.CartItems` tables in isolated `cart` schema

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | `c8c74c80` | Cart domain model and value objects |
| 2 | `00700bdd` | EF Core configurations, CartDbContext update, and migration |

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| `Domain.Entities.Cart` fully qualified in DbContext/Configuration | Avoids namespace collision with `Cart` infrastructure namespace |
| Private EF Core constructors on all entities | Follows StockItem/StockReservation pattern for encapsulation |
| CartItem is a regular class, not an aggregate root | Owned by Cart aggregate, not independently accessible |

## Key Files

### Created
- `src/MicroCommerce.ApiService/Features/Cart/Domain/ValueObjects/CartId.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Domain/ValueObjects/CartItemId.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/CartItem.cs`
- `src/MicroCommerce.ApiService/Features/Cart/BuyerIdentity.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Configurations/CartConfiguration.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Configurations/CartItemConfiguration.cs`
- `src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Migrations/20260209112915_InitialCart.cs`

### Modified
- `src/MicroCommerce.ApiService/Features/Cart/Infrastructure/CartDbContext.cs`

## Verification

- [x] `dotnet build src/MicroCommerce.ApiService/` succeeds
- [x] Cart.cs contains Create, AddItem, UpdateItemQuantity, RemoveItem methods
- [x] CartItem.cs contains Create, IncrementQuantity, SetQuantity methods
- [x] BuyerIdentity.cs contains GetOrCreateBuyerId with cookie handling
- [x] Migration file exists in Features/Cart/Infrastructure/Migrations/
- [x] CartDbContext has DbSet<Cart> and DbSet<CartItem>
- [x] Max quantity 99 enforced in Cart and CartItem
- [x] 30-day TTL resets on every modification via Touch()

## Duration

~2 minutes
