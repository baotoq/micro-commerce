---
phase: 19-specification-pattern
plan: 01
subsystem: api
tags: [ardalis-specification, cqrs, ddd, query-specification, efcore, catalog]

# Dependency graph
requires:
  - phase: 18-enumeration-enums-with-behavior
    provides: ProductStatus SmartEnum used in PublishedProductsSpec and ProductByStatusSpec
  - phase: 16.1-adopt-vogen-for-value-object
    provides: CategoryId Vogen value object used in ProductsByCategorySpec
provides:
  - Ardalis.Specification 9.3.1 in BuildingBlocks.Common (base spec infrastructure)
  - Ardalis.Specification.EntityFrameworkCore 9.3.1 in ApiService (WithSpecification EF Core extension)
  - 6 catalog specification classes (ProductsBaseSpec, PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec, ProductByStatusSpec, GetProductsFilterSpec)
  - GetProductsQueryHandler refactored from inline Where chains to WithSpecification(spec)
affects: [phase-19-plan-02, testing]

# Tech tracking
tech-stack:
  added:
    - "Ardalis.Specification 9.3.1 (BuildingBlocks.Common)"
    - "Ardalis.Specification.EntityFrameworkCore 9.3.1 (MicroCommerce.ApiService)"
  patterns:
    - "Specification objects with Ardalis fluent Query.Where() builder"
    - "Multiple Query.Where() calls in single spec constructor (Ardalis AND semantics)"
    - "WithSpecification(spec) extension on DbSet for count and list queries"
    - "Sorting and pagination kept in handler (not in specs) to allow reusable count queries"
    - "Composite spec (GetProductsFilterSpec) takes optional parameters for flexible composition"

key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductsBaseSpec.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/PublishedProductsSpec.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductsByCategorySpec.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductSearchSpec.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductByStatusSpec.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/GetProductsFilterSpec.cs
  modified:
    - src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs

key-decisions:
  - "Ardalis.Specification 9.3.1 does not expose And()/Or() extension methods — multiple Query.Where() calls in single spec constructor achieves equivalent AND semantics"
  - "GetProductsFilterSpec uses composite spec with optional constructor parameters instead of spec.And(spec) chain (consistent with Ardalis v9 API)"
  - "Sorting kept in handler (request-specific SortBy/SortDirection params) to avoid double-ordering with handler-level sort switch"
  - "Pagination (Skip/Take) kept in handler so same spec reusable for both count and list queries"
  - "Ardalis.Specification placed in BuildingBlocks.Common (no EF Core dependency); Ardalis.Specification.EntityFrameworkCore placed in ApiService only"

patterns-established:
  - "Pattern: Spec filtering only — handler owns sorting, pagination, projection (Join + Select stays in handler)"
  - "Pattern: WithSpecification(spec) applied twice — once for CountAsync, once for list query (same spec, no duplication)"
  - "Pattern: Composite spec via optional constructor parameters — replaces conditional Where chain in handler"

requirements-completed: [QUERY-01, QUERY-02]

# Metrics
duration: 3min
completed: 2026-02-24
---

# Phase 19 Plan 01: Specification Pattern Summary

**Ardalis.Specification 9.3.1 installed with 6 catalog spec classes and GetProductsQueryHandler refactored from inline Where chains to WithSpecification(spec) composition**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-24T15:49:58Z
- **Completed:** 2026-02-24T15:52:58Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Installed Ardalis.Specification 9.3.1 in BuildingBlocks.Common (base spec, no EF dependency) and Ardalis.Specification.EntityFrameworkCore 9.3.1 in ApiService (WithSpecification extension)
- Created 6 catalog specification classes: ProductsBaseSpec (identity), PublishedProductsSpec (Published status filter), ProductsByCategorySpec (CategoryId filter), ProductSearchSpec (name/description/sku search), ProductByStatusSpec (arbitrary status filter), GetProductsFilterSpec (composite with optional params)
- Refactored GetProductsQueryHandler to use WithSpecification(spec) for both count and list queries, replacing all inline Where chains while preserving API contract (ProductListDto shape, pagination, sorting, Join/Select projection unchanged)

## Task Commits

Each task was committed atomically:

1. **Task 1: Install packages and create catalog spec classes** - `6a7c895b` (feat)
2. **Task 2: Refactor GetProductsQueryHandler to use specification composition** - `a037ce74` (feat)

## Files Created/Modified
- `src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj` - Added Ardalis.Specification 9.3.1
- `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` - Added Ardalis.Specification.EntityFrameworkCore 9.3.1
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductsBaseSpec.cs` - Identity spec (no filter, starting point for composition)
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/PublishedProductsSpec.cs` - Filters by Published status
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductsByCategorySpec.cs` - Filters by CategoryId
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductSearchSpec.cs` - Filters by name/description/sku search term
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/ProductByStatusSpec.cs` - Filters by arbitrary ProductStatus
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Specifications/GetProductsFilterSpec.cs` - Composite spec with optional category/status/search params
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs` - Refactored to use WithSpecification(spec)

## Decisions Made
- **Ardalis.Specification 9.3.1 And() absence:** The plan specified `spec.And(otherSpec)` composition, but Ardalis.Specification 9.3.1 does not expose an `And()` extension method on `Specification<T>`. (A naming conflict with `NpgsqlFullTextSearchLinqExtensions.And()` would also have caused compile errors.) Solution: used a single `GetProductsFilterSpec` with optional constructor parameters that calls `Query.Where()` conditionally — Ardalis ANDs multiple Where calls automatically. This achieves equivalent AND semantics while being idiomatic for the actual v9 API.
- **Sorting in handler not specs:** Per the plan's explicit guidance, sorting is handled in the handler via `request.SortBy/SortDirection` switch — not in specifications — to avoid double-ordering conflicts.
- **Package layering:** Ardalis.Specification (no EF Core dependency) in BuildingBlocks.Common; Ardalis.Specification.EntityFrameworkCore in ApiService only. Confirmed no EntityFrameworkCore reference leaked into BuildingBlocks.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Ardalis.Specification 9.3.1 missing And() method / Npgsql naming conflict**
- **Found during:** Task 2 (Refactor GetProductsQueryHandler)
- **Issue:** Build error: `'Specification<Product>' does not contain a definition for 'And' and the best extension method overload 'NpgsqlFullTextSearchLinqExtensions.And(NpgsqlTsQuery, NpgsqlTsQuery)' requires a receiver of type 'NpgsqlTsQuery'`. Ardalis.Specification 9.3.1 does not have an `And()` extension on `Specification<T>`.
- **Fix:** Created `GetProductsFilterSpec` with optional constructor parameters (categoryId, status, searchTerm) using multiple conditional `Query.Where()` calls — Ardalis ANDs them automatically. Replaced the `spec.And(...)` chain approach with this composite spec.
- **Files modified:** `GetProductsFilterSpec.cs` (new), `GetProductsQueryHandler.cs` (adapted)
- **Verification:** Build succeeds with 0 errors; `WithSpecification(spec)` confirmed in handler via grep
- **Committed in:** `a037ce74` (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - API mismatch)
**Impact on plan:** Equivalent AND filtering semantics achieved via composite spec. All must_haves satisfied: WithSpecification used in handler, spec classes created, API contract unchanged, packages installed at correct layers. The individual spec classes (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec, ProductByStatusSpec) remain unit-testable via IsSatisfiedBy().

## Issues Encountered
- Ardalis.Specification 9.3.1 does not have `And()` composition — resolved by using composite spec pattern with multiple `Query.Where()` calls (Ardalis v9 idiomatic approach).

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Specification pattern infrastructure established; ready for Phase 19 Plan 02 (Ordering specifications for GetAllOrdersQueryHandler and GetOrdersByBuyerQueryHandler)
- 6 individual spec classes remain available for unit testing via IsSatisfiedBy() when test infrastructure is established (Phase 21)
- Established composite spec pattern (GetProductsFilterSpec) can be used as template for ordering specs in next plan

## Self-Check: PASSED

- All 9 files created/modified confirmed on disk
- Commits 6a7c895b and a037ce74 confirmed in git log
- Ardalis.Specification in BuildingBlocks.Common confirmed
- Ardalis.Specification.EntityFrameworkCore in ApiService confirmed
- WithSpecification in handler confirmed
- Join projection preserved confirmed

---
*Phase: 19-specification-pattern*
*Completed: 2026-02-24*
