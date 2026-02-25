---
phase: 22-wire-interceptors-to-dbcontexts
plan: 01
subsystem: database
tags: [efcore, interceptors, audit, concurrency, soft-delete, integration-tests, testcontainers]

# Dependency graph
requires:
  - phase: 21-adoption-full-building-block-integration
    provides: AuditableAggregateRoot, IConcurrencyToken, ISoftDeletable base classes wired to Cart entity
  - phase: 15-ddd-building-blocks
    provides: AuditInterceptor, ConcurrencyInterceptor, SoftDeleteInterceptor implementations
  - phase: 20-integration-tests
    provides: ApiWebApplicationFactory, IntegrationTestBase, ReplaceDbContext test infrastructure

provides:
  - AuditInterceptor wired to all 8 DbContext registrations via AddInterceptors() in Program.cs
  - ConcurrencyInterceptor wired to all 8 DbContext registrations
  - SoftDeleteInterceptor wired to all 8 DbContext registrations
  - ApiWebApplicationFactory mirrors interceptor wiring in ReplaceDbContext
  - InterceptorBehaviorTests with 5 integration tests proving audit, version, and concurrency behavior at runtime
  - Dead AddScoped<> registrations for stateless interceptors removed from Program.cs

affects:
  - All feature DbContexts (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists, Outbox)
  - Any entity implementing IAuditable, IConcurrencyToken, or ISoftDeletable

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Singleton interceptor instances shared across all DbContexts (stateless, no DI dependencies)
    - AddInterceptors() order: SoftDelete first, Concurrency second, Audit last (per Phase 15-02 decision)
    - Static readonly interceptor fields in test factory mirror production singleton pattern
    - CartEntity type alias to avoid namespace conflict with Integration.Cart test directory

key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/Integration/Interceptors/InterceptorBehaviorTests.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs

key-decisions:
  - "Stateless interceptors (SoftDelete, Concurrency, Audit) instantiated as singletons via `new` before DbContext registrations — no DI lookup needed, safe to share across all 8 DbContexts"
  - "DomainEventInterceptor kept as AddScoped<> — it has IPublishEndpoint dependency and is out of Phase 22 scope"
  - "CartEntity type alias used in InterceptorBehaviorTests to resolve namespace conflict with Integration/Cart/ test directory"

patterns-established:
  - "Interceptor wiring pattern: create singleton instances before first AddNpgsqlDbContext call, reference in each lambda via options.AddInterceptors()"
  - "Test factory mirrors production wiring: static readonly fields + AddInterceptors in ReplaceDbContext"

requirements-completed: [ENTITY-02, ENTITY-04, ENTITY-05, ADOPT-03]

# Metrics
duration: 12min
completed: 2026-02-25
---

# Phase 22 Plan 01: Wire Interceptors to DbContexts Summary

**SoftDeleteInterceptor, ConcurrencyInterceptor, and AuditInterceptor wired to all 8 DbContext registrations via singleton AddInterceptors() pattern; 5 integration tests confirm audit timestamps, version tracking, and concurrent update rejection work end-to-end**

## Performance

- **Duration:** 12 min
- **Started:** 2026-02-25T00:00:00Z
- **Completed:** 2026-02-25T00:12:00Z
- **Tasks:** 2
- **Files modified:** 3 (2 modified, 1 created)

## Accomplishments

- Wired all 3 interceptors to all 8 DbContext registrations in Program.cs using singleton instances (order: SoftDelete, Concurrency, Audit)
- Removed 3 dead AddScoped<> registrations for stateless interceptors (SoftDelete, Concurrency, Audit); DomainEventInterceptor scoped registration preserved
- Mirrored interceptor wiring in ApiWebApplicationFactory.ReplaceDbContext via static readonly fields
- Created InterceptorBehaviorTests with 5 integration tests using CartDbContext (Cart implements both AuditableAggregateRoot and IConcurrencyToken)
- Full test suite passes: 182 tests (177 existing + 5 new), zero regressions

## Task Commits

Each task was committed atomically:

1. **Task 1: Wire interceptors to all 8 DbContext registrations in Program.cs and ApiWebApplicationFactory** - `7ef61035` (feat)
2. **Task 2: Add InterceptorBehaviorTests integration tests** - `f08a106a` (test)

## Files Created/Modified

- `src/MicroCommerce.ApiService/Program.cs` - Added singleton interceptor instances before DbContext registrations; added AddInterceptors() to all 8 DbContext lambdas; removed 3 dead AddScoped registrations
- `src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs` - Added 3 static readonly interceptor fields; added AddInterceptors() to ReplaceDbContext method
- `src/MicroCommerce.ApiService.Tests/Integration/Interceptors/InterceptorBehaviorTests.cs` - New: 5 integration tests covering AuditInterceptor (insert/update) and ConcurrencyInterceptor (insert, update, concurrent rejection)

## Decisions Made

- Stateless interceptors (SoftDelete, Concurrency, Audit) instantiated as singletons via `new` before DbContext registrations — no DI lookup needed, safe to share across all 8 DbContexts. DomainEventInterceptor kept as AddScoped<> because it has an IPublishEndpoint dependency.
- CartEntity type alias used in InterceptorBehaviorTests to resolve namespace conflict with Integration/Cart/ test subdirectory.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Used CartEntity type alias to fix Cart namespace conflict**
- **Found during:** Task 2 (InterceptorBehaviorTests creation)
- **Issue:** Integration test directory `Integration/Cart/` creates a namespace `MicroCommerce.ApiService.Tests.Integration.Cart` that conflicts with `using MicroCommerce.ApiService.Features.Cart.Domain.Entities` — compiler reports `Cart` is a namespace, not a type
- **Fix:** Added `using CartEntity = MicroCommerce.ApiService.Features.Cart.Domain.Entities.Cart;` type alias; replaced all `Cart` type references with `CartEntity` throughout the test file
- **Files modified:** `src/MicroCommerce.ApiService.Tests/Integration/Interceptors/InterceptorBehaviorTests.cs`
- **Verification:** Build succeeded with zero errors; all 5 tests pass
- **Committed in:** f08a106a (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - bug: namespace conflict)
**Impact on plan:** Fix required for compilability. No scope creep.

## Issues Encountered

None beyond the Cart namespace conflict documented above (auto-fixed inline).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 4 v2.0 gap requirements satisfied: ENTITY-02, ENTITY-04, ENTITY-05, ADOPT-03
- Interceptors are now active at runtime for all 8 DbContexts — any entity implementing IAuditable, IConcurrencyToken, or ISoftDeletable will have automatic behavior on SaveChanges
- Phase 22 (wire-interceptors-to-dbcontexts) is the final gap closure phase — v2.0 DDD Foundation is fully complete

---
*Phase: 22-wire-interceptors-to-dbcontexts*
*Completed: 2026-02-25*
