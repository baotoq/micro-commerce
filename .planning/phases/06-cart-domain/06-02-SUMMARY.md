---
phase: 06-cart-domain
plan: 02
subsystem: cart
tags: [cqrs, api-endpoints, background-service, minimal-api]
dependency-graph:
  requires: [06-01]
  provides: [cart-api-endpoints, cart-cqrs-handlers, cart-expiration-service]
  affects: [06-03, 06-04]
tech-stack:
  added: []
  patterns: [cqrs-command-query, background-service, cookie-identity, upsert-pattern]
key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/AddToCart/AddToCartCommand.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/AddToCart/AddToCartCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/AddToCart/AddToCartValidator.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/UpdateCartItem/UpdateCartItemCommand.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/UpdateCartItem/UpdateCartItemCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/RemoveCartItem/RemoveCartItemCommand.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/RemoveCartItem/RemoveCartItemCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Queries/GetCart/CartDto.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Queries/GetCart/GetCartQuery.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Queries/GetCart/GetCartQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Queries/GetCartItemCount/GetCartItemCountQuery.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Queries/GetCartItemCount/GetCartItemCountQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Cart/CartEndpoints.cs
    - src/MicroCommerce.ApiService/Features/Cart/Infrastructure/CartExpirationService.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - id: cart-expiration-executedelete
    title: "ExecuteDeleteAsync for cart expiration (not load-then-remove)"
    rationale: "More efficient bulk delete - single SQL statement instead of loading entities"
metrics:
  duration: ~2 minutes
  completed: 2026-02-09
---

# Phase 06 Plan 02: Cart CQRS & API Endpoints Summary

**One-liner:** CQRS handlers for all cart operations, 5 minimal API endpoints with cookie-based buyer identity, and hourly cart expiration background service.

## What Was Built

### Task 1: Cart CQRS Handlers (Commands and Queries)

Created 5 CQRS handler pairs following the existing Inventory module patterns:

- **AddToCartCommand** - Upsert logic: loads cart by BuyerId, creates if null via `Cart.Create()`, calls `cart.AddItem()`. Returns `AddToCartResult(IsUpdate)` to indicate new vs incremented.
- **AddToCartValidator** - Validates ProductId not empty, ProductName not empty, UnitPrice > 0, Quantity 1-99.
- **UpdateCartItemCommand** - Loads cart by BuyerId with Items, calls `cart.UpdateItemQuantity()`. Throws `InvalidOperationException` if cart not found.
- **RemoveCartItemCommand** - Loads cart by BuyerId with Items, calls `cart.RemoveItem()`. Throws `InvalidOperationException` if cart not found.
- **GetCartQuery** - Returns `CartDto` with Items projected to `CartItemDto` (includes `LineTotal = UnitPrice * Quantity`), `TotalPrice`, and `TotalItems`. Returns null if no cart found.
- **GetCartItemCountQuery** - Lightweight query using `SelectMany` + `SumAsync` to get total item count without loading entities.

### Task 2: Cart Endpoints, Expiration Service, Program.cs Registration

**CartEndpoints.cs** - 5 routes under `/api/cart`:
- `GET /` - Returns cart with items or 204 No Content
- `POST /items` - Adds item (creates cart on first use)
- `PUT /items/{itemId:guid}` - Updates item quantity
- `DELETE /items/{itemId:guid}` - Removes item
- `GET /count` - Returns total item count for header badge

All endpoints use `BuyerIdentity.GetOrCreateBuyerId(httpContext)` for buyer resolution. No `RequireAuthorization` - cart works for guests.

**CartExpirationService.cs** - BackgroundService with 1-hour interval that uses `ExecuteDeleteAsync` for efficient bulk removal of expired carts (past 30-day TTL).

**Program.cs** - Added `app.MapCartEndpoints()` and `builder.Services.AddHostedService<CartExpirationService>()`.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 8d26f1c1 | Cart CQRS handlers (commands and queries) |
| 2 | b4d6e115 | Cart endpoints, expiration service, and Program.cs registration |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Used ExecuteDeleteAsync instead of load-then-remove for cart expiration**
- **Found during:** Task 2 (CartExpirationService)
- **Issue:** Plan referenced ReservationCleanupService pattern which loads entities then removes. For cart expiration, bulk delete is more efficient since we don't need entity state.
- **Fix:** Used `ExecuteDeleteAsync` for single-statement SQL delete of expired carts.
- **Files:** CartExpirationService.cs

## Verification

- [x] `dotnet build src/MicroCommerce.ApiService/` succeeds
- [x] Program.cs contains `app.MapCartEndpoints()` and `AddHostedService<CartExpirationService>()`
- [x] CartEndpoints.cs maps GET /, POST /items, PUT /items/{itemId}, DELETE /items/{itemId}, GET /count
- [x] All handlers follow existing CQRS patterns with ISender
- [x] CartExpirationService follows BackgroundService pattern
- [x] AddToCartValidator validates ProductId, ProductName, UnitPrice, Quantity

## Next Phase Readiness

Plan 06-02 is complete. The cart API is fully functional. Ready for:
- **06-03**: Storefront cart UI (Add to Cart button, cart drawer/page)
- **06-04**: Cart integration testing or additional cart features
