---
phase: 13-wishlists-saved-items
plan: 02
subsystem: application
tags: [cqrs, mediatr, rest-api, minimal-api, authentication]

# Dependency graph
requires:
  - phase: 13-wishlists-saved-items
    plan: 01
    provides: "WishlistItem entity and WishlistsDbContext"
provides:
  - "AddToWishlistCommand with idempotent upsert behavior"
  - "RemoveFromWishlistCommand with idempotent delete behavior"
  - "GetUserWishlistQuery with cross-context batch lookups"
  - "GetWishlistCountQuery for wishlist item count"
  - "GetWishlistProductIdsQuery for heart icon state"
  - "5 authenticated REST API endpoints at /api/wishlist"
affects: [13-03]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Cross-context batch queries to prevent N+1 problems"
    - "Idempotent commands (add returns existing ID, remove ignores not found)"
    - "Minimal API endpoints with RequireAuthorization()"
    - "GetUserId helper pattern for JWT claim extraction"

key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/AddToWishlist/AddToWishlistCommand.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/RemoveFromWishlist/RemoveFromWishlistCommand.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetUserWishlist/GetUserWishlistQuery.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetWishlistCount/GetWishlistCountQuery.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetWishlistProductIds/GetWishlistProductIdsQuery.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/WishlistsEndpoints.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs

key-decisions:
  - "Idempotent commands: AddToWishlist returns existing ID if already in wishlist, RemoveFromWishlist silently succeeds if not found"
  - "Cross-context batch queries: GetUserWishlistQuery batch-loads product details from CatalogDbContext and stock info from InventoryDbContext to avoid N+1"
  - "GetWishlistProductIdsQuery returns just product IDs for efficient heart icon state (frontend loads once, checks membership in Set)"
  - "All 5 endpoints require authentication via RequireAuthorization() on the route group"

patterns-established:
  - "Wishlists CQRS handlers follow existing Reviews/Cart patterns with MediatR"
  - "Cross-context batch query pattern for performance optimization"
  - "Idempotent command pattern for reliable API operations"

# Metrics
duration: 3min
completed: 2026-02-13
---

# Phase 13 Plan 02: Wishlists CQRS Handlers & REST API Summary

**Complete wishlist API with 2 idempotent commands, 3 queries (including cross-context batch lookups), and 5 authenticated REST endpoints**

## Performance

- **Duration:** 3 minutes
- **Started:** 2026-02-13T13:35:17Z
- **Completed:** 2026-02-13T13:38:27Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- Created AddToWishlistCommand with idempotent upsert (returns existing ID if already in wishlist)
- Created RemoveFromWishlistCommand with idempotent delete (no error if not found)
- Created GetUserWishlistQuery with cross-context batch lookups to CatalogDbContext (product details) and InventoryDbContext (stock info)
- Created GetWishlistCountQuery for wishlist item count
- Created GetWishlistProductIdsQuery for efficient heart icon state (returns just product IDs)
- Created WishlistsEndpoints with 5 REST API endpoints requiring authentication
- Registered MapWishlistsEndpoints() in Program.cs

## Task Commits

Each task was committed atomically:

1. **Task 1: Create CQRS commands and queries for wishlist operations** - `63b75f1e` (feat)
2. **Task 2: Create WishlistsEndpoints and register in Program.cs** - `33c3caec` (feat)

## Files Created/Modified
- `src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/AddToWishlist/AddToWishlistCommand.cs` - Idempotent add command (returns existing ID if already in wishlist)
- `src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/RemoveFromWishlist/RemoveFromWishlistCommand.cs` - Idempotent remove command (no error if not found)
- `src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetUserWishlist/GetUserWishlistQuery.cs` - Full wishlist query with cross-context batch lookups (product details, stock info, ratings)
- `src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetWishlistCount/GetWishlistCountQuery.cs` - Simple count query for badge display
- `src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetWishlistProductIds/GetWishlistProductIdsQuery.cs` - Optimized query returning just product IDs for heart icon state
- `src/MicroCommerce.ApiService/Features/Wishlists/WishlistsEndpoints.cs` - 5 REST API endpoints (GET list, GET count, GET product-ids, POST add, DELETE remove) all requiring authentication
- `src/MicroCommerce.ApiService/Program.cs` - Added Wishlists using statement and MapWishlistsEndpoints() call

## Decisions Made
- **Idempotent commands:** AddToWishlistCommand checks if item already exists and returns existing ID instead of throwing error. RemoveFromWishlistCommand silently succeeds if item not found. This prevents errors from duplicate UI interactions or race conditions.
- **Cross-context batch queries:** GetUserWishlistQuery follows Reviews pattern - batch-loads all product details from CatalogDbContext and all stock info from InventoryDbContext in two queries instead of N+1. Filters out deleted products gracefully.
- **Efficient product IDs query:** GetWishlistProductIdsQuery returns just product IDs for frontend to load once and check membership in a Set. Avoids N+1 API calls per product to check heart icon state.
- **Authentication required:** All endpoints require authentication via RequireAuthorization() on the route group, consistent with Reviews pattern.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Backend wishlist API complete and ready for frontend integration (Plan 13-03):
- AddToWishlist and RemoveFromWishlist commands are idempotent
- GetUserWishlist provides full product details with stock info and ratings
- GetWishlistCount for badge display
- GetWishlistProductIds for efficient heart icon state
- All endpoints authenticated and registered
- Cross-context batch queries prevent N+1 performance problems

No blockers or concerns.

## Self-Check: PASSED

All created files verified:
- FOUND: AddToWishlistCommand.cs
- FOUND: RemoveFromWishlistCommand.cs
- FOUND: GetUserWishlistQuery.cs
- FOUND: GetWishlistCountQuery.cs
- FOUND: GetWishlistProductIdsQuery.cs
- FOUND: WishlistsEndpoints.cs
- FOUND: Program.cs updated with MapWishlistsEndpoints()

All commits verified:
- FOUND: 63b75f1e (Task 1)
- FOUND: 33c3caec (Task 2)

---
*Phase: 13-wishlists-saved-items*
*Completed: 2026-02-13*
