---
phase: 16-conventions-dry-configuration
plan: 01
subsystem: database
tags: [efcore, conventions, IModelFinalizingConvention, snake_case, strongly-typed-id, soft-delete, concurrency-token, auditable]

# Dependency graph
requires:
  - phase: 15-foundation-entity-base-audit-infrastructure
    provides: StronglyTypedId<T>, IAuditable, IConcurrencyToken, ISoftDeletable interfaces and static convention helpers
provides:
  - BaseDbContext abstract class with all conventions auto-registered
  - StronglyTypedIdConvention: auto value converter for all StronglyTypedId<T> properties
  - AuditableConvention: auto timestamp with time zone column type for IAuditable
  - ConcurrencyTokenConvention: auto IsConcurrencyToken flag for IConcurrencyToken
  - SoftDeletableConvention: auto IsDeleted query filter for ISoftDeletable
  - snake_case naming convention applied globally via EFCore.NamingConventions
  - All 8 DbContexts inherit BaseDbContext receiving all conventions automatically
affects:
  - 16-02 (configuration cleanup that removes per-entity manual HasConversion calls)
  - any future phase adding new DbContexts

# Tech tracking
tech-stack:
  added: [EFCore.NamingConventions 10.0.1]
  patterns:
    - IModelFinalizingConvention for cross-cutting EF Core configuration
    - BaseDbContext as shared ancestor for all feature DbContexts
    - Reflection-based value converter generation using Expression trees

key-files:
  created:
    - src/MicroCommerce.ApiService/Common/Persistence/Conventions/StronglyTypedIdConvention.cs
    - src/MicroCommerce.ApiService/Common/Persistence/Conventions/AuditableConvention.cs
    - src/MicroCommerce.ApiService/Common/Persistence/Conventions/ConcurrencyTokenConvention.cs
    - src/MicroCommerce.ApiService/Common/Persistence/Conventions/SoftDeletableConvention.cs
    - src/MicroCommerce.ApiService/Common/Persistence/BaseDbContext.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/CatalogDbContext.cs
    - src/MicroCommerce.ApiService/Features/Cart/Infrastructure/CartDbContext.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/OrderingDbContext.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Infrastructure/InventoryDbContext.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Infrastructure/ProfilesDbContext.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/ReviewsDbContext.cs
    - src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/WishlistsDbContext.cs
    - src/MicroCommerce.ApiService/Common/Persistence/OutboxDbContext.cs
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj

key-decisions:
  - "IModelFinalizingConvention over ModelBuilder extension methods: proper EF Core convention API for cross-cutting config"
  - "Conventions.ConcurrencyTokenConvention qualifier in BaseDbContext to avoid naming collision with deleted static class"
  - "Expression tree approach for StronglyTypedId value converters: avoids boxing, works with all concrete StronglyTypedId<T> subtypes"
  - "EFCore.NamingConventions 10.0.1 compatible with EF Core 10.0.0 for snake_case naming"

patterns-established:
  - "All new DbContexts must inherit BaseDbContext (not DbContext directly)"
  - "Cross-cutting EF Core config goes in Conventions/ via IModelFinalizingConvention"
  - "StronglyTypedId properties require no manual HasConversion - auto-detected by convention"

requirements-completed: []

# Metrics
duration: 7min
completed: 2026-02-24
---

# Phase 16 Plan 01: EF Core Model Conventions and BaseDbContext Summary

**4 IModelFinalizingConvention classes and BaseDbContext with EFCore.NamingConventions eliminating per-entity EF Core boilerplate across all 8 DbContexts**

## Performance

- **Duration:** 7 min
- **Started:** 2026-02-24T10:00:57Z
- **Completed:** 2026-02-24T10:07:57Z
- **Tasks:** 2
- **Files modified:** 14 (5 created, 9 modified, 2 deleted)

## Accomplishments
- Created 4 IModelFinalizingConvention implementations covering StronglyTypedId, IAuditable, IConcurrencyToken, ISoftDeletable
- Created BaseDbContext registering all conventions plus snake_case naming (EFCore.NamingConventions)
- Migrated all 8 DbContexts from DbContext to BaseDbContext inheritance
- Deleted 2 Phase 15 static convention helpers (replaced by proper IModelFinalizingConvention)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create IModelFinalizingConvention implementations and BaseDbContext** - `b51b5294` (feat)
2. **Task 2: Migrate all DbContexts to inherit BaseDbContext and remove Phase 15 static helpers** - `8a4ce94e` (feat)

**Plan metadata:** (docs commit — see final commit)

## Files Created/Modified
- `src/MicroCommerce.ApiService/Common/Persistence/Conventions/StronglyTypedIdConvention.cs` - Auto value converter for all StronglyTypedId<T> properties via reflection and Expression trees
- `src/MicroCommerce.ApiService/Common/Persistence/Conventions/AuditableConvention.cs` - Auto timestamp with time zone column type for IAuditable.CreatedAt/UpdatedAt
- `src/MicroCommerce.ApiService/Common/Persistence/Conventions/ConcurrencyTokenConvention.cs` - Auto IsConcurrencyToken(true) for IConcurrencyToken.Version
- `src/MicroCommerce.ApiService/Common/Persistence/Conventions/SoftDeletableConvention.cs` - Auto IsDeleted query filter for ISoftDeletable (skips derived types)
- `src/MicroCommerce.ApiService/Common/Persistence/BaseDbContext.cs` - Abstract base registering all 4 conventions and UseSnakeCaseNamingConvention
- `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` - Added EFCore.NamingConventions 10.0.1
- All 8 DbContexts changed to inherit BaseDbContext

**Deleted:**
- `src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyTokenConvention.cs` - Phase 15 static helper (replaced)
- `src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteQueryFilterConvention.cs` - Phase 15 static helper (replaced)

## Decisions Made
- **IConventionModelBuilder namespace fix:** `IConventionModelBuilder` lives in `Microsoft.EntityFrameworkCore.Metadata.Builders` (not `Conventions`), required adding the `Builders` using directive.
- **Name collision resolution:** `ConcurrencyTokenConvention` class name exists in both old `Common.Persistence` namespace and new `Conventions` sub-namespace. Used `Conventions.ConcurrencyTokenConvention()` qualifier in BaseDbContext to resolve ambiguity until old static file was deleted in Task 2.
- **Expression tree for StronglyTypedId:** Used `Expression.Lambda` with `GetConstructor` to build `ValueConverter<TId, TUnderlying>` dynamically — works for all concrete subtypes without boxing.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Added missing `Microsoft.EntityFrameworkCore.Metadata.Builders` using directive**
- **Found during:** Task 1 (Create IModelFinalizingConvention implementations)
- **Issue:** `IConventionModelBuilder` type used in `ProcessModelFinalizing` signature lives in the `Builders` namespace, not `Conventions` namespace. Build error CS0246 on first compile.
- **Fix:** Added `using Microsoft.EntityFrameworkCore.Metadata.Builders;` to all 4 convention files
- **Files modified:** All 4 convention files in Conventions/
- **Verification:** Build passed with zero errors after fix
- **Committed in:** b51b5294 (Task 1 commit)

**2. [Rule 1 - Bug] Resolved ConcurrencyTokenConvention namespace collision in BaseDbContext**
- **Found during:** Task 1 (BaseDbContext creation)
- **Issue:** Old static `ConcurrencyTokenConvention` in parent namespace shadowed new convention class, causing CS0712 (cannot instantiate static class)
- **Fix:** Used `Conventions.ConcurrencyTokenConvention()` qualifier in BaseDbContext until old static helper was deleted in Task 2
- **Files modified:** BaseDbContext.cs
- **Verification:** Build passed with zero errors
- **Committed in:** b51b5294 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 1 - Bug)
**Impact on plan:** Both fixes necessary to resolve compilation errors. No scope creep. Plan executed as designed.

## Issues Encountered
- EFCore.NamingConventions required exact namespace lookup for `IConventionModelBuilder` — resolved by inspection via small test project.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- BaseDbContext conventions foundation complete, ready for Plan 02 configuration cleanup
- Plan 02 can now safely remove manual `HasConversion` calls from entity configurations since StronglyTypedIdConvention handles them automatically
- All 8 DbContexts are convention-compliant

---
*Phase: 16-conventions-dry-configuration*
*Completed: 2026-02-24*
