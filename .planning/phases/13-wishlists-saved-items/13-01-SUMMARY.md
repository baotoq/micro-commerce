---
phase: 13-wishlists-saved-items
plan: 01
subsystem: database
tags: [postgres, entity-framework, domain-model, database-per-feature]

# Dependency graph
requires:
  - phase: 12-reviews-ratings
    provides: "Reviews feature with separate DbContext pattern"
provides:
  - "WishlistItem entity with strongly-typed ID"
  - "WishlistsDbContext with 'wishlists' schema isolation"
  - "Composite unique index enforcing one wishlist entry per user per product"
  - "Database migration for WishlistItems table"
affects: [13-02, 13-03]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Database-per-feature with dedicated schema (wishlists)"
    - "Composite unique constraint on (UserId, ProductId)"
    - "Simple entity without aggregate root or domain events"

key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Wishlists/Domain/Entities/WishlistItem.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Domain/ValueObjects/WishlistItemId.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/WishlistsDbContext.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Configurations/WishlistItemConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Migrations/20260213133155_InitialWishlists.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs

key-decisions:
  - "WishlistItem is a simple entity (not aggregate root) with no domain events"
  - "Composite unique index on (UserId, ProductId) enforces single entry per user-product pair"
  - "Three indexes: composite unique, UserId for listing, AddedAt descending for chronological sort"

patterns-established:
  - "Wishlists feature follows database-per-feature pattern with dedicated schema"
  - "Simple entities without domain logic use plain entity class (not BaseAggregateRoot)"

# Metrics
duration: 2min
completed: 2026-02-13
---

# Phase 13 Plan 01: Wishlists Domain Model & Database Infrastructure Summary

**WishlistItem entity with composite unique constraint on (UserId, ProductId) using dedicated 'wishlists' schema and PostgreSQL xmin concurrency**

## Performance

- **Duration:** 2 minutes
- **Started:** 2026-02-13T13:30:19Z
- **Completed:** 2026-02-13T13:32:30Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Created WishlistItem entity as simple entity (not aggregate root) with UserId, ProductId, AddedAt properties
- Established WishlistsDbContext with dedicated 'wishlists' schema following database-per-feature pattern
- Generated InitialWishlists migration with composite unique index on (UserId, ProductId)
- Registered WishlistsDbContext in Program.cs with separate migration history table

## Task Commits

Each task was committed atomically:

1. **Task 1: Create WishlistItem entity and WishlistItemId value object** - `9a38d8b5` (feat)
2. **Task 2: Create WishlistsDbContext, configuration, migration, and register in Program.cs** - `00fcc46a` (feat)

## Files Created/Modified
- `src/MicroCommerce.ApiService/Features/Wishlists/Domain/Entities/WishlistItem.cs` - WishlistItem entity with Create factory method, UserId, ProductId, AddedAt properties
- `src/MicroCommerce.ApiService/Features/Wishlists/Domain/ValueObjects/WishlistItemId.cs` - Strongly-typed ID with New() factory method
- `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/WishlistsDbContext.cs` - DbContext with 'wishlists' schema isolation
- `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Configurations/WishlistItemConfiguration.cs` - EF Core configuration with composite unique index, UserId index, and AddedAt descending index
- `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Migrations/20260213133155_InitialWishlists.cs` - Migration creating WishlistItems table with all indexes
- `src/MicroCommerce.ApiService/Program.cs` - Added WishlistsDbContext registration with dedicated migration history table

## Decisions Made
- **WishlistItem as simple entity:** Unlike Review (aggregate root with domain events), WishlistItem is a simple entity with no business logic or domain events - just a join table tracking user-product relationships
- **Composite unique index:** (UserId, ProductId) composite unique constraint enforces single wishlist entry per user-product pair at database level
- **Three indexes for query patterns:** Composite unique index for constraint enforcement, UserId index for "get my wishlist" queries, AddedAt descending for chronological sorting

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Database infrastructure complete and ready for CQRS handlers (Plan 13-02):
- WishlistItem entity with all required properties
- WishlistsDbContext registered and configured
- Migration generated with proper indexes
- Composite unique constraint prevents duplicate entries
- Ready for AddToWishlist, RemoveFromWishlist, GetUserWishlist commands/queries

No blockers or concerns.

## Self-Check: PASSED

All created files verified:
- FOUND: WishlistItem.cs
- FOUND: WishlistItemId.cs
- FOUND: WishlistsDbContext.cs
- FOUND: WishlistItemConfiguration.cs
- FOUND: InitialWishlists migration

All commits verified:
- FOUND: 9a38d8b5 (Task 1)
- FOUND: 00fcc46a (Task 2)

---
*Phase: 13-wishlists-saved-items*
*Completed: 2026-02-13*
