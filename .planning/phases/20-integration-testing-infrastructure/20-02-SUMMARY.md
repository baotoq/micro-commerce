---
phase: 20-integration-testing-infrastructure
plan: 02
subsystem: testing
tags: [xunit, integration-testing, test-builders, fluent-results, handler-testing, profiles, reviews, wishlists]

# Dependency graph
requires:
  - phase: 20-01
    provides: IntegrationTestBase, FakeAuthenticationHandler, ApiWebApplicationFactory
provides:
  - ProductBuilder and OrderBuilder fluent test data builders
  - ProfilesEndpointsTests: authenticated user auto-profile creation test
  - ReviewsEndpointsTests: public endpoint empty list test
  - WishlistsEndpointsTests: authenticated empty wishlist test
  - UpdateOrderStatusHandlerTests: handler-level FluentResults business rule testing
  - All 7 features now have at least one representative integration test
affects:
  - future test plans: builders available for seeding complex test scenarios

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Handler-level test: seed via DbContext directly, resolve IMediator.Send(), assert result.IsFailed"
    - "OrderBuilder fluent API: creates Order in Submitted status via Order.Create() factory method"
    - "ProductBuilder fluent API: creates Product via Product.Create() with value object wrappers"
    - "FluentResults assertion: result.IsFailed.Should().BeTrue(), result.Errors[0].Message checks"

key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/Integration/Builders/ProductBuilder.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Builders/OrderBuilder.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Profiles/ProfilesEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Reviews/ReviewsEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Wishlists/WishlistsEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Ordering/UpdateOrderStatusHandlerTests.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Reviews/ReviewsEndpoints.cs

key-decisions:
  - "UpdateOrderStatusHandlerTests uses IMediator.Send() not direct handler instantiation: DI resolves all dependencies including OrderingDbContext and FluentResults pipeline behaviors"
  - "Handle_NonExistentOrder_ThrowsNotFoundException not ReturnsFailResult: handler throws NotFoundException (not Result.Fail) for missing orders — NotFoundException is caught by exception middleware at endpoint level"
  - "ReviewsEndpoints.GetProductReviews params moved after ISender/CancellationToken with defaults: same fix as CatalogEndpoints in 20-01 — Minimal API requires non-service params at end with defaults for optional query params"

requirements-completed: [TEST-01]

# Metrics
duration: 3min
completed: 2026-02-25
---

# Phase 20 Plan 02: Integration Testing Infrastructure — Feature Tests Summary

**Test data builders (ProductBuilder, OrderBuilder) and representative integration tests for all 7 features — 34 integration tests green, handler-level FluentResults pattern established**

## Performance

- **Duration:** ~3 min
- **Started:** 2026-02-25
- **Completed:** 2026-02-25
- **Tasks:** 2
- **Files modified:** 7 (6 created, 1 modified)

## Accomplishments

- Created ProductBuilder fluent test data builder with factory method wrapping value objects (ProductName, Money, CategoryId)
- Created OrderBuilder fluent test data builder creating Order in Submitted status via Order.Create()
- Created ProfilesEndpointsTests: authenticated GET /api/profiles/me returns auto-created profile for new user
- Created ReviewsEndpointsTests: public GET /api/reviews/products/{productId} returns empty list (no auth needed)
- Created WishlistsEndpointsTests: authenticated GET /api/wishlist/ returns empty list for new user
- Created UpdateOrderStatusHandlerTests: handler-level tests with direct DB seeding and FluentResults assertions
- All 178 tests pass (34 integration + 144 unit)

## Task Commits

Each task was committed atomically:

1. **Task 1: Test data builders and 3 feature endpoint tests** - `a44cfcad` (feat)
2. **Task 2: Handler-level test for UpdateOrderStatusCommandHandler** - `69b7a430` (feat)

## Files Created/Modified

- `src/MicroCommerce.ApiService.Tests/Integration/Builders/ProductBuilder.cs` - NEW: fluent builder for Product test data
- `src/MicroCommerce.ApiService.Tests/Integration/Builders/OrderBuilder.cs` - NEW: fluent builder for Order test data (Submitted status)
- `src/MicroCommerce.ApiService.Tests/Integration/Profiles/ProfilesEndpointsTests.cs` - NEW: authenticated user auto-profile creation test
- `src/MicroCommerce.ApiService.Tests/Integration/Reviews/ReviewsEndpointsTests.cs` - NEW: public endpoint empty reviews test
- `src/MicroCommerce.ApiService.Tests/Integration/Wishlists/WishlistsEndpointsTests.cs` - NEW: authenticated empty wishlist test
- `src/MicroCommerce.ApiService.Tests/Integration/Ordering/UpdateOrderStatusHandlerTests.cs` - NEW: handler-level FluentResults business rule tests
- `src/MicroCommerce.ApiService/Features/Reviews/ReviewsEndpoints.cs` - Fixed missing query param defaults for page/pageSize

## Decisions Made

- UpdateOrderStatusHandlerTests resolves IMediator from DI (not direct handler construction) for full pipeline support
- Handle_NonExistentOrder_ThrowsNotFoundException documents that handler throws (not Result.Fail) for not-found — exception middleware converts to 404 at endpoint boundary
- Reviews endpoint page/pageSize params moved after ISender/CancellationToken with defaults (= 1, = 5) — same as CatalogEndpoints fix in 20-01

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] ReviewsEndpoints.GetProductReviews returns 400 with no query params**
- **Found during:** Task 1 (running integration tests)
- **Issue:** `int page` and `int pageSize` params had no default values. Minimal API returns 400 Bad Request when query params are omitted. The endpoint had internal defaults (`if (page < 1) page = 1`) but binding fails before reaching that code.
- **Fix:** Moved ISender and CancellationToken before page/pageSize params, added `= 1` and `= 5` defaults matching the internal fallback logic
- **Files modified:** ReviewsEndpoints.cs
- **Verification:** ReviewsEndpointsTests.GetProductReviews_NoReviews_ReturnsEmptyList passes
- **Committed in:** a44cfcad (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 bug)
**Impact on plan:** Single production bug fix required for Reviews test to pass. Same pattern as CatalogEndpoints fix in 20-01.

## Test Coverage by Feature

| Feature | Test Class | Test Pattern | Auth |
|---------|------------|--------------|------|
| Catalog | CatalogEndpointsTests | CRUD lifecycle | Guest |
| Cart | CartEndpointsTests | Add/update/clear | Guest+Cookie |
| Ordering | OrderingEndpointsTests | Checkout + status | Guest+Cookie |
| Ordering (handler) | UpdateOrderStatusHandlerTests | FluentResults business rules | Direct DB+Mediator |
| Inventory | InventoryEndpointsTests | Stock management | Guest |
| Profiles | ProfilesEndpointsTests | Auto-create profile | Authenticated |
| Reviews | ReviewsEndpointsTests | Public list endpoint | Guest |
| Wishlists | WishlistsEndpointsTests | Empty wishlist | Authenticated |

## Issues Encountered

None beyond the auto-fixed Reviews endpoint bug.

## Next Phase Readiness

- All 7 features have representative integration tests
- Handler-level testing pattern established (direct DB seeding + IMediator + FluentResults assertions)
- Test data builders (ProductBuilder, OrderBuilder) ready for complex test scenarios
- Phase 20 complete — integration testing infrastructure finished

---
*Phase: 20-integration-testing-infrastructure*
*Completed: 2026-02-25*
