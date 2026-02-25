---
phase: 21-adoption-full-building-block-integration
plan: 02
subsystem: database
tags: [concurrency, ef-core, migrations, postgres, entity, building-blocks, optimistic-locking]

# Dependency graph
requires:
  - phase: 21-01
    provides: "AuditableAggregateRoot/Entity<TId> adoption across 5 aggregates and 5 child entities"
  - phase: 15-01
    provides: "IConcurrencyToken interface definition with int Version { get; set; }"
  - phase: 15-02
    provides: "ConcurrencyInterceptor auto-incrementing Version, ConcurrencyTokenConvention marking it as token"
provides:
  - "All 6 entities with PostgreSQL xmin-based concurrency migrated to portable IConcurrencyToken int Version"
  - "EF Core migrations generated for Cart, Ordering, Inventory, Profiles, Reviews, Wishlists DbContexts"
  - "WishlistItem gains Entity<WishlistItemId> base class as part of ADOPT-01 scope"
affects:
  - "21-03: remaining building block adoptions in same phase"
  - "Any future concurrency conflict handling (DbUpdateConcurrencyException -> HTTP 409)"

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IConcurrencyToken with int Version replaces PostgreSQL-specific xmin uint [Timestamp]"
    - "AddColumn version migration pattern (not RenameColumn) because xmin is a PostgreSQL system column"
    - "ConcurrencyTokenConvention auto-detects IConcurrencyToken and marks Version as concurrency token"
    - "ConcurrencyInterceptor auto-increments Version on Added (set to 1) and Modified (increment)"

key-files:
  created:
    - "src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Migrations/20260225104508_AddExplicitVersionColumn.cs"
    - "src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/20260225104518_AddExplicitVersionColumn.cs"
    - "src/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Migrations/20260225104526_AddExplicitVersionColumn.cs"
    - "src/MicroCommerce.ApiService/Migrations/20260225104536_AddExplicitVersionColumn.cs"
    - "src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Migrations/20260225104545_AddExplicitVersionColumn.cs"
    - "src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Migrations/20260225104554_AddExplicitVersionColumn.cs"
  modified:
    - "src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs"
    - "src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs"
    - "src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockItem.cs"
    - "src/MicroCommerce.ApiService/Features/Profiles/Domain/Entities/UserProfile.cs"
    - "src/MicroCommerce.ApiService/Features/Reviews/Domain/Entities/Review.cs"
    - "src/MicroCommerce.ApiService/Features/Wishlists/Domain/Entities/WishlistItem.cs"

key-decisions:
  - "xmin to version migration uses AddColumn (not RenameColumn): xmin is a PostgreSQL system column that PostgreSQL forbids renaming; EF Core auto-generates incorrect RenameColumn that must be manually corrected"
  - "WishlistItem gains Entity<WishlistItemId> base class alongside IConcurrencyToken: consistent with ADOPT-01 scope for standalone entities with Id"
  - "Version column defaults to 0 in migration; ConcurrencyInterceptor sets it to 1 on first save, increments on each update"

patterns-established:
  - "PostgreSQL xmin migration pattern: AddColumn version integer NOT NULL DEFAULT 0 (never RenameColumn xmin)"

requirements-completed:
  - ADOPT-03

# Metrics
duration: 5min
completed: 2026-02-25
---

# Phase 21 Plan 02: IConcurrencyToken Adoption Summary

**6 entities migrated from PostgreSQL-specific xmin [Timestamp] uint to portable IConcurrencyToken int Version with 6 EF Core migrations replacing the system column mapping**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-25T10:43:18Z
- **Completed:** 2026-02-25T10:48:14Z
- **Tasks:** 2
- **Files modified:** 24 (6 entities + 6 migration .cs + 6 migration .Designer.cs + 6 model snapshots)

## Accomplishments
- All 6 entities (Cart, Order, StockItem, UserProfile, Review, WishlistItem) implement IConcurrencyToken with `int Version { get; set; }`
- `[Timestamp] uint Version` removed from all 6 entities; `System.ComponentModel.DataAnnotations` import removed
- EF Core migrations generated for all 6 DbContexts; each migration adds `version INTEGER NOT NULL DEFAULT 0`
- WishlistItem also gains `Entity<WishlistItemId>` base class for consistent ADOPT-01 hierarchy
- All 177 tests pass after migrations are applied via MigrateAsync

## Task Commits

Each task was committed atomically:

1. **Task 1: Migrate 6 entities from xmin [Timestamp] uint to IConcurrencyToken int Version** - `bbddc0ac` (feat)
2. **Task 2: Generate EF Core migrations for IConcurrencyToken schema changes** - `fb756207` (feat)

## Files Created/Modified
- `src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs` - Removed [Timestamp] uint, added IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs` - Removed [Timestamp] uint, added IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockItem.cs` - Removed [Timestamp] uint, added IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Profiles/Domain/Entities/UserProfile.cs` - Removed [Timestamp] uint, added IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Reviews/Domain/Entities/Review.cs` - Removed [Timestamp] uint, added IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Wishlists/Domain/Entities/WishlistItem.cs` - Added Entity<WishlistItemId> base, IConcurrencyToken int Version
- `src/MicroCommerce.ApiService/Features/Cart/Infrastructure/Migrations/20260225104508_AddExplicitVersionColumn.cs` - AddColumn version integer
- `src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/20260225104518_AddExplicitVersionColumn.cs` - AddColumn version integer
- `src/MicroCommerce.ApiService/Features/Inventory/Infrastructure/Migrations/20260225104526_AddExplicitVersionColumn.cs` - AddColumn version integer
- `src/MicroCommerce.ApiService/Migrations/20260225104536_AddExplicitVersionColumn.cs` - AddColumn version integer (Profiles)
- `src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Migrations/20260225104545_AddExplicitVersionColumn.cs` - AddColumn version integer
- `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Migrations/20260225104554_AddExplicitVersionColumn.cs` - AddColumn version integer

## Decisions Made
- **xmin migration uses AddColumn not RenameColumn**: PostgreSQL's xmin is a system column that cannot be renamed. EF Core auto-generates `RenameColumn xmin -> version` which fails with `0A000: cannot rename system column "xmin"`. Correct approach: `AddColumn version` since xmin was a virtual mapping, not a physical column.
- **WishlistItem gets Entity<WishlistItemId> base class**: Consistent with ADOPT-01 scope; WishlistItem is a standalone entity with an Id, warranting the Entity base class.
- **Version defaults to 0 in DB**: Migration sets DEFAULT 0; ConcurrencyInterceptor sets to 1 on first save. Existing rows get version=0 which is fine (they'll be set to 1 on next update).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed invalid migration: RenameColumn xmin -> version fails in PostgreSQL**
- **Found during:** Task 2 (Generate EF Core migrations)
- **Issue:** EF Core auto-generated `RenameColumn("xmin", ...)` in all 6 migration Up() methods. PostgreSQL system columns cannot be renamed, causing `0A000: cannot rename system column "xmin"` in integration tests.
- **Fix:** Replaced all 6 migration Up()/Down() bodies with `AddColumn<int> version` (Up) and `DropColumn version` (Down). xmin was never a physical column — it was a virtual PostgreSQL system column mapping.
- **Files modified:** All 6 AddExplicitVersionColumn.cs migration files
- **Verification:** All 177 tests pass after fix (integration tests apply migrations via MigrateAsync)
- **Committed in:** `fb756207` (Task 2 commit, included with migration files)

---

**Total deviations:** 1 auto-fixed (Rule 1 - Bug in generated migration SQL)
**Impact on plan:** Auto-fix necessary for correctness. EF Core's migration generator doesn't understand PostgreSQL xmin system column semantics. No scope creep.

## Issues Encountered
- EF Core migration generator incorrectly treats xmin as a real column and generates RenameColumn. PostgreSQL forbids renaming system columns. Fixed by manually rewriting migration Up/Down bodies to use AddColumn/DropColumn instead.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- ADOPT-03 complete: all 6 entities with xmin concurrency now use portable IConcurrencyToken int Version
- ConcurrencyTokenConvention and ConcurrencyInterceptor are live and handling Version automatically
- Ready for Plan 03: next building block adoption in Phase 21

---
*Phase: 21-adoption-full-building-block-integration*
*Completed: 2026-02-25*
