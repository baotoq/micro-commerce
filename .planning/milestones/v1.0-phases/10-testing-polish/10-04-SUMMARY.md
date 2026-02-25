---
phase: 10-testing-polish
plan: 04
subsystem: testing
tags: [integration-tests, testcontainers, webapplicationfactory, api-testing]
requires: [10-01]
provides: [api-integration-tests, testcontainers-setup]
affects: []
tech-stack:
  added: []
  patterns: [WebApplicationFactory, Testcontainers, Integration Testing]
key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/IntegrationTestCollection.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Catalog/CatalogEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Cart/CartEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Inventory/InventoryEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Ordering/OrderingEndpointsTests.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Cart/Aggregates/CartTests.cs
decisions:
  - decision: "WebApplicationFactory with Testcontainers PostgreSQL"
    rationale: "Integration tests require real database to verify SQL, migrations, and EF Core mappings"
  - decision: "MassTransit test harness instead of real Service Bus"
    rationale: "Integration tests focus on HTTP contract, not saga execution"
  - decision: "No-op IImageUploadService stub"
    rationale: "Image upload tests don't need actual Azure Blob Storage"
  - decision: "Disable background services in tests"
    rationale: "Data seeders and cleanup services interfere with test isolation"
  - decision: "Seed test data directly via DbContext"
    rationale: "Tests control exact database state for predictable assertions"
  - decision: "Cookie-based buyer identity in Cart/Ordering tests"
    rationale: "Tests don't use real authentication, cookie simulates guest buyer flow"
  - decision: "PostgreSQL 15 Alpine image"
    rationale: "Lighter weight than full postgres image, sufficient for tests"
metrics:
  duration: 607
  completed: 2026-02-12
---

# Phase 10 Plan 04: API Integration Tests Summary

Integration test infrastructure with Testcontainers PostgreSQL and 30 API endpoint tests across all feature modules

## What Was Built

### Task 1: Integration Test Infrastructure
**ApiWebApplicationFactory:**
- Custom WebApplicationFactory<Program> with IAsyncLifetime
- PostgreSQL Testcontainer (postgres:15-alpine) lifecycle management
- Replaced all 5 DbContexts (Outbox, Catalog, Cart, Ordering, Inventory) with test container connection
- MassTransit replaced with AddMassTransitTestHarness() for in-memory testing
- Azure Blob Storage stubbed with NoOpImageUploadService
- Disabled background services (data seeders, cleanup) for test isolation
- InitializeAsync applies all EF migrations automatically

**IntegrationTestCollection:**
- xUnit collection fixture sharing one PostgreSQL container across test classes
- Significant performance improvement over per-test container creation

**Bug Fixes (Rule 1 - Auto-fix bugs):**
- Fixed CartTests.cs namespace conflict (Domain.Entities.Cart vs test namespace ending in .Cart)
- Fixed CheckoutStateMachineTests.cs saga harness access (.Saga property on ISagaInstance)
- Fixed PostgreSqlBuilder obsolete constructor (now requires image parameter)

**Program.cs Enhancement:**
- Added `public partial class Program { }` to make Program accessible for WebApplicationFactory

### Task 2: API Endpoint Integration Tests
**Catalog (8 tests):**
- GET /api/catalog/products (empty database)
- POST /api/catalog/categories (valid request)
- GET /api/catalog/categories (returns categories)
- POST /api/catalog/products (valid, invalid)
- GET /api/catalog/products/{id} (exists, 404)
- PUT /api/catalog/products/{id} (update product)

**Cart (7 tests):**
- GET /api/cart (empty cart, after add)
- POST /api/cart/items (add to cart)
- PUT /api/cart/items/{id} (update quantity)
- DELETE /api/cart/items/{id} (remove item)
- GET /api/cart/count (empty, with items)

**Inventory (6 tests):**
- GET /api/inventory/stock?productIds={ids} (existing, missing)
- POST /api/inventory/stock/{id}/adjust (valid, non-existent)
- POST /api/inventory/stock/{id}/reserve (valid, insufficient stock)

**Ordering (9 tests):**
- POST /api/ordering/checkout (valid, invalid)
- GET /api/ordering/orders/{id} (exists, 404)
- GET /api/ordering/orders/my (buyer orders)
- GET /api/ordering/orders (all orders)
- GET /api/ordering/dashboard (stats)
- PATCH /api/ordering/orders/{id}/status (update status)

**Total: 30 integration tests** covering all feature module API endpoints

## Deviations from Plan

### Auto-fixed Issues (Deviation Rules 1-3)

**1. [Rule 1 - Bug] CartTests namespace conflict**
- **Found during:** Task 1 build verification
- **Issue:** `Domain.Entities.Cart` couldn't be resolved because test namespace ends with `.Cart`, causing conflict
- **Fix:** Added type alias `using CartAggregate = MicroCommerce.ApiService.Features.Cart.Domain.Entities.Cart`
- **Files modified:** CartTests.cs
- **Commit:** eadd17cb

**2. [Rule 1 - Bug] CheckoutStateMachineTests saga harness type mismatch**
- **Found during:** Task 1 build verification
- **Issue:** Previous fix attempt incorrectly accessed `.Saga` property, but original code was correct
- **Fix:** Reverted file to last commit state
- **Files modified:** CheckoutStateMachineTests.cs
- **Commit:** eadd17cb (revert via git checkout)

**3. [Rule 1 - Bug] PostgreSqlBuilder obsolete constructor**
- **Found during:** Task 1 build
- **Issue:** Parameterless constructor deprecated, now requires image parameter
- **Fix:** Changed `new PostgreSqlBuilder().WithImage("postgres:15-alpine")` to `new PostgreSqlBuilder("postgres:15-alpine")`
- **Files modified:** ApiWebApplicationFactory.cs
- **Commit:** eadd17cb

**4. [Rule 2 - Missing Critical] AddToCartResult doesn't return ItemId**
- **Found during:** Task 2 implementation
- **Issue:** Tests assumed AddToCartResult.ItemId property, but actual DTO only has IsUpdate boolean
- **Fix:** Modified tests to get cart and extract item ID from CartDto response instead
- **Files modified:** CartEndpointsTests.cs
- **Commit:** 7133d21f

**5. [Rule 2 - Missing Critical] StockItem.Create signature**
- **Found during:** Task 2 implementation
- **Issue:** Tests called `StockItem.Create(productId, quantity)` but actual signature is `Create(productId)` only
- **Fix:** Modified tests to create stock item then call `AdjustStock(quantity, reason)` to set initial stock
- **Files modified:** InventoryEndpointsTests.cs
- **Commit:** 7133d21f

**6. [Rule 2 - Missing Critical] OrderDto.Email vs BuyerEmail**
- **Found during:** Task 2 implementation
- **Issue:** Tests referenced `OrderDto.Email` but actual property is `BuyerEmail`
- **Fix:** Updated assertions to use correct property name
- **Files modified:** OrderingEndpointsTests.cs
- **Commit:** 7133d21f

**7. [Rule 2 - Missing Critical] OrderDashboardDto.TotalRevenue vs Revenue**
- **Found during:** Task 2 implementation
- **Issue:** Tests referenced non-existent `TotalRevenue` property
- **Fix:** Corrected to use `Revenue` property
- **Files modified:** OrderingEndpointsTests.cs
- **Commit:** 7133d21f

None of these deviations required architectural decisions - all were straightforward fixes to align tests with actual implementation.

## Key Technical Decisions

1. **Testcontainers over in-memory database**: Real PostgreSQL ensures tests match production behavior (SQL dialects, migrations, concurrency)

2. **Shared container via collection fixture**: One PostgreSQL container for all tests significantly faster than per-test containers

3. **Direct DbContext seeding**: Tests inject DbContext and seed data directly for precise control over test state

4. **Cookie-based buyer identity**: Cart and Ordering tests set `buyer_id` cookie to simulate guest buyer without real auth

5. **MassTransit test harness**: Integration tests verify HTTP contracts, not saga execution, so in-memory harness sufficient

## Next Phase Readiness

**Blockers:** None

**Concerns:**
- Integration tests require Docker running (Testcontainers)
- Tests not executed in this session (Docker unavailable)
- Tests compile successfully but actual execution pending Docker availability

**Follow-up:**
- Run `dotnet test --filter "Category=Integration"` with Docker running to verify all 30 tests pass
- Consider adding GitHub Actions workflow with Docker for CI/CD integration test execution

## Files Modified

### Created (6 files)
```
src/MicroCommerce.ApiService.Tests/Integration/
├── Fixtures/
│   ├── ApiWebApplicationFactory.cs         (165 lines, infrastructure)
│   └── IntegrationTestCollection.cs        (11 lines, xUnit fixture)
├── Catalog/
│   └── CatalogEndpointsTests.cs            (216 lines, 8 tests)
├── Cart/
│   └── CartEndpointsTests.cs               (191 lines, 7 tests)
├── Inventory/
│   └── InventoryEndpointsTests.cs          (143 lines, 6 tests)
└── Ordering/
    └── OrderingEndpointsTests.cs           (234 lines, 9 tests)
```

### Modified (2 files)
```
src/MicroCommerce.ApiService/
└── Program.cs                              (+2 lines, public partial class)

src/MicroCommerce.ApiService.Tests/Unit/Cart/Aggregates/
└── CartTests.cs                            (+1 line, type alias)
```

## Commits

- `eadd17cb` - feat(10-04): create integration test infrastructure with Testcontainers
- `7133d21f` - feat(10-04): write integration tests for all API endpoints

**Total:** 2 commits, ~950 lines added
