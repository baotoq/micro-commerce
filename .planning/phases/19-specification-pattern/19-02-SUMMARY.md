---
phase: 19-specification-pattern
plan: 02
subsystem: api
tags: [ardalis-specification, cqrs, ddd, query-specification, efcore, ordering, smartenum]

# Dependency graph
requires:
  - phase: 19-specification-pattern
    plan: 01
    provides: Ardalis.Specification packages installed, WithSpecification pattern established
  - phase: 18-enumeration-enums-with-behavior
    provides: OrderStatus SmartEnum used in ActiveOrdersSpec TerminalStatuses array
provides:
  - 2 ordering specification classes (ActiveOrdersSpec, OrdersByBuyerSpec)
  - GetAllOrdersQueryHandler refactored from inline Where to WithSpecification(spec)
  - GetOrdersByBuyerQueryHandler refactored with chained WithSpecification calls for composition
affects: [testing, phase-20, phase-21]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "ActiveOrdersSpec with TerminalStatuses array excludes Failed/Cancelled by default (QUERY-03 pattern)"
    - "Optional statusFilter narrowing via second Query.Where() in ActiveOrdersSpec constructor"
    - "Spec composition via chained WithSpecification() calls (buyer spec then active orders spec)"
    - "WithSpecification applied to same IQueryable for both CountAsync and list queries"

key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Specifications/ActiveOrdersSpec.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Specifications/OrdersByBuyerSpec.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQueryHandler.cs

key-decisions:
  - "Spec composition via chained WithSpecification() calls instead of And() — consistent with Plan 01 decision that Ardalis.Specification 9.3.1 And() is unavailable due to Npgsql naming conflict"
  - "ActiveOrdersSpec uses static readonly TerminalStatuses array for Contains() check — clean, reusable terminal status definition"
  - "Sorting (OrderByDescending) and pagination (Skip/Take) kept in handlers not specs — identical to catalog pattern established in Plan 01"

requirements-completed: [QUERY-03]

# Metrics
duration: 2min
completed: 2026-02-24
---

# Phase 19 Plan 02: Specification Pattern Summary

**2 ordering specification classes created and 2 ordering query handlers refactored to use WithSpecification composition for buyer-scoped active order filtering**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-24T15:57:10Z
- **Completed:** 2026-02-24T15:59:10Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Created `ActiveOrdersSpec` with static `TerminalStatuses` array excluding `OrderStatus.Failed` and `OrderStatus.Cancelled` by default, with optional status narrowing via second `Query.Where()` call
- Created `OrdersByBuyerSpec` filtering orders by `Guid buyerId` — single-responsibility buyer filter
- Refactored `GetAllOrdersQueryHandler` to use `ActiveOrdersSpec` via `WithSpecification(spec)`, replacing inline conditional `Where` + status parse logic
- Refactored `GetOrdersByBuyerQueryHandler` to compose `OrdersByBuyerSpec` + `ActiveOrdersSpec` via chained `WithSpecification(buyerSpec).WithSpecification(activeSpec)`, replacing inline buyer + status filtering
- Both handlers preserve identical API contract: `OrderListDto`/`OrderSummaryDto` shape, pagination, `OrderByDescending` sorting, and `Select` projection unchanged

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ordering specification classes** - `e06432d0` (feat)
2. **Task 2: Refactor ordering query handlers to use specifications** - `9763e3d9` (feat)

## Files Created/Modified
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Specifications/ActiveOrdersSpec.cs` - Excludes terminal statuses, optional status narrowing
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Specifications/OrdersByBuyerSpec.cs` - Filters by buyer ID
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs` - Refactored to use `ActiveOrdersSpec` via `WithSpecification`
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQueryHandler.cs` - Refactored to chain `OrdersByBuyerSpec` + `ActiveOrdersSpec` via `WithSpecification`

## Decisions Made
- **Chained WithSpecification instead of And():** The plan specified `spec.And(otherSpec)` composition in `GetOrdersByBuyerQueryHandler`, but Ardalis.Specification 9.3.1 does not expose `And()` (naming conflict with `NpgsqlFullTextSearchLinqExtensions.And()`, as documented in Plan 01). Solution: chained two `WithSpecification()` calls on the same `IQueryable<Order>` — `WithSpecification(buyerSpec).WithSpecification(activeSpec)` — EF Core applies both filter expressions as AND conditions. Achieves equivalent semantics without `And()`.
- **Static TerminalStatuses array:** Defined as `private static readonly OrderStatus[] TerminalStatuses = [OrderStatus.Failed, OrderStatus.Cancelled]` in `ActiveOrdersSpec` — centralizes terminal status definition, aligns with QUERY-03 requirement.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Plan used And() composition which is unavailable in Ardalis.Specification 9.3.1**
- **Found during:** Task 2 (Refactor GetOrdersByBuyerQueryHandler)
- **Issue:** Plan's Task 2 code sample used `spec.And(new ActiveOrdersSpec(request.Status))` which fails to compile — Ardalis.Specification 9.3.1 does not expose `And()` and Npgsql's `NpgsqlFullTextSearchLinqExtensions.And()` causes ambiguity (documented in Plan 01 deviation).
- **Fix:** Used chained `WithSpecification(buyerSpec).WithSpecification(activeSpec)` calls in the handler — `WithSpecification` returns `IQueryable<T>` so chaining applies both specs as EF Core AND conditions. This is functionally equivalent to `And()` composition.
- **Files modified:** `GetOrdersByBuyerQueryHandler.cs` (adapted approach)
- **Verification:** Build succeeds with 0 errors; `WithSpecification` confirmed in both handlers via grep; projections and API contract preserved
- **Committed in:** `9763e3d9` (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - API mismatch, consistent with Plan 01 decision)
**Impact on plan:** Equivalent AND filtering semantics achieved via chained WithSpecification. All must_haves satisfied: WithSpecification used in both handlers, spec classes created, API contract unchanged, SmartEnum OrderStatus works correctly in Contains() check.

## Issues Encountered
- Ardalis.Specification 9.3.1 `And()` unavailable — resolved by chaining `WithSpecification()` calls (consistent with Plan 01 established pattern).

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Specification pattern fully adopted across both catalog and ordering domains
- Phase 19 complete: QUERY-01, QUERY-02, QUERY-03 requirements fulfilled
- Specification classes available for unit testing via `IsSatisfiedBy()` when test infrastructure established (Phase 21)
- Pattern established for any future query handlers needing specification-based filtering

## Self-Check: PASSED

- `ActiveOrdersSpec.cs` confirmed on disk
- `OrdersByBuyerSpec.cs` confirmed on disk
- `GetAllOrdersQueryHandler.cs` contains `WithSpecification` confirmed
- `GetOrdersByBuyerQueryHandler.cs` contains two `WithSpecification` calls confirmed
- Commits `e06432d0` and `9763e3d9` confirmed in git log
- Both handlers contain `.Select(o => new OrderSummaryDto` projection confirmed
- Build: 0 errors, 0 task-related warnings

---
*Phase: 19-specification-pattern*
*Completed: 2026-02-24*
