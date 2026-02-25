---
phase: 20-integration-testing-infrastructure
plan: 01
subsystem: testing
tags: [xunit, testcontainers, webapplicationfactory, masstransit, efcore, integration-testing, fake-auth]

# Dependency graph
requires:
  - phase: 19-specification-pattern
    provides: query handlers and domain model used by integration tests
provides:
  - Fixed ApiWebApplicationFactory with all 8 DbContexts registered, MassTransit test harness, and FakeAuthenticationHandler
  - IntegrationTestBase with CreateAuthenticatedClient, CreateGuestClient, CreateScope, ResetDatabase helpers
  - Per-schema DROP+MigrateAsync test isolation pattern
  - All 29 existing integration tests green (0 failures)
affects:
  - 20-02: builds on top of this test infrastructure

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Per-schema test isolation: DROP SCHEMA CASCADE then MigrateAsync per DbContext type"
    - "FakeAuthenticationHandler: X-Test-UserId header injects sub+NameIdentifier claims"
    - "ReplaceDbContext helper: removes pool services, re-registers with AddDbContext + UseSnakeCaseNamingConvention"
    - "MassTransit health check dedup via PostConfigure on HealthCheckServiceOptions"
    - "IntegrationTestBase: abstract class with IAsyncLifetime, override InitializeAsync for per-class schema reset"

key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/FakeAuthenticationHandler.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/IntegrationTestBase.cs
  modified:
    - src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Catalog/CatalogEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Integration/Ordering/OrderingEndpointsTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Inventory/Aggregates/StockItemTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Ordering/Aggregates/OrderTests.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetCategories/GetCategoriesQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Catalog/CatalogEndpoints.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockItem.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderDashboard/GetOrderDashboardQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderStatus.cs

key-decisions:
  - "MigrateAsync over EnsureCreated for multi-schema shared PostgreSQL: EnsureCreated skips if any tables exist; MigrateAsync uses per-schema __EFMigrationsHistory tables independently"
  - "DROP SCHEMA IF EXISTS CASCADE per context for per-class reset: avoids EnsureDeletedAsync which drops entire database including other schemas"
  - "UseSnakeCaseNamingConvention required in ReplaceDbContext helper: production code applies it via BaseDbContext, tests need it too for column name matching"
  - "MassTransit health check dedup via PostConfigure not pre-filtering: AddMassTransitTestHarness re-adds health checks even after pre-removal; PostConfigure on HealthCheckServiceOptions deduplicates after all registrations complete"
  - "SmartEnum [JsonConverter] on class for client-side deserialization: HttpClient GetFromJsonAsync needs converter at type level, not just server-side serialization"
  - "StockItem.Reserve throws ConflictException (409) not InvalidOperationException (400): insufficient stock is a conflict, not a bad request"
  - "GetOrderDashboard groups in memory not SQL: EF/Npgsql cannot translate DateTimeOffset.Date + GroupBy + DateOnly.FromDateTime to SQL"
  - "DateTimeOffset UTC construction: new DateTimeOffset(UtcDateTime.Date, TimeSpan.Zero) avoids Npgsql rejection of non-UTC values"
  - "EF1002 pragma disable on ExecuteSqlRawAsync: schema name comes from EF model metadata (not user input), safe to suppress"

patterns-established:
  - "IntegrationTestBase pattern: extend for per-class DB isolation, override InitializeAsync, call ResetDatabase(typeof(SomeDbContext))"
  - "Fake auth: X-Test-UserId header on HttpClient, FakeAuthenticationHandler injects sub+NameIdentifier claims"
  - "ReplaceDbContext<T>(services): removes pool descriptors by FullName string matching, re-registers with AddDbContext"

requirements-completed: [TEST-01]

# Metrics
duration: 90min
completed: 2026-02-25
---

# Phase 20 Plan 01: Integration Testing Infrastructure Summary

**xUnit WebApplicationFactory fixed with per-schema PostgreSQL isolation, FakeAuthenticationHandler for Keycloak bypass, and IntegrationTestBase helpers — all 29 existing integration tests green**

## Performance

- **Duration:** ~90 min
- **Started:** 2026-02-25
- **Completed:** 2026-02-25
- **Tasks:** 2
- **Files modified:** 12 (2 created, 10 modified)

## Accomplishments

- Fixed ApiWebApplicationFactory: MassTransit health check dedup, UseSnakeCaseNamingConvention in ReplaceDbContext, MigrateAsync per-schema isolation
- Created FakeAuthenticationHandler for Keycloak bypass via X-Test-UserId header
- Created IntegrationTestBase with CreateAuthenticatedClient, CreateGuestClient, CreateScope, ResetDatabase helpers
- Fixed 10 production and test bugs discovered during test runs (all auto-fixed per deviation rules)
- All 29 integration tests pass (Catalog 8, Cart 7, Ordering 8, Inventory 6)
- All 144 unit tests pass (173 total, 0 failures)

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix ApiWebApplicationFactory and add FakeAuthenticationHandler** - `3172c8b9` (feat)
2. **Task 2: Create IntegrationTestBase and verify all existing tests pass** - `4be777c2` (feat)

## Files Created/Modified

- `src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs` - Fixed factory: MassTransit health check dedup, UseSnakeCaseNamingConvention, per-schema MigrateAsync, FakeAuth registration
- `src/MicroCommerce.ApiService.Tests/Integration/Fixtures/FakeAuthenticationHandler.cs` - NEW: X-Test-UserId header -> sub+NameIdentifier claims
- `src/MicroCommerce.ApiService.Tests/Integration/Fixtures/IntegrationTestBase.cs` - NEW: abstract base with CreateAuthenticatedClient, CreateGuestClient, CreateScope, ResetDatabase
- `src/MicroCommerce.ApiService.Tests/Integration/Catalog/CatalogEndpointsTests.cs` - Extended IntegrationTestBase, added ResetDatabase(CatalogDbContext) for isolation
- `src/MicroCommerce.ApiService.Tests/Integration/Ordering/OrderingEndpointsTests.cs` - Fixed UpdateOrderStatus test to accept 422 (Result pattern returns UnprocessableEntity)
- `src/MicroCommerce.ApiService.Tests/Unit/Inventory/Aggregates/StockItemTests.cs` - Updated to expect ConflictException instead of InvalidOperationException
- `src/MicroCommerce.ApiService.Tests/Unit/Ordering/Aggregates/OrderTests.cs` - Fixed 9 error message patterns to match TransitionTo format
- `src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetCategories/GetCategoriesQueryHandler.cs` - Fixed .OrderBy(c => c.Name) instead of .Name.Value (LINQ translation)
- `src/MicroCommerce.ApiService/Features/Catalog/CatalogEndpoints.cs` - Added default values for page/pageSize params (was returning 400 without them)
- `src/MicroCommerce.ApiService/Features/Inventory/Domain/Entities/StockItem.cs` - Changed Reserve() to throw ConflictException (409) for insufficient stock
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderDashboard/GetOrderDashboardQueryHandler.cs` - Fixed DateTimeOffset UTC; fetch timestamps then group in memory
- `src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderStatus.cs` - Added [JsonConverter] attribute for client-side SmartEnum deserialization

## Decisions Made

- MigrateAsync per DbContext (with per-schema __EFMigrationsHistory) replaces EnsureCreated which fails with multi-schema shared PostgreSQL
- Per-class schema reset uses DROP SCHEMA CASCADE + MigrateAsync, not EnsureDeletedAsync (which drops entire database)
- MassTransit health check dedup via PostConfigure on HealthCheckServiceOptions (post-filtering works, pre-removal does not)
- SmartEnum needs [JsonConverter] at class level for client-side HttpClient deserialization
- StockItem.Reserve throws ConflictException not InvalidOperationException (insufficient stock is 409 Conflict)
- GetOrderDashboard groups timestamps in memory (EF cannot translate DateTimeOffset.Date + GroupBy + DateOnly.FromDateTime)

## Deviations from Plan

The plan specified EnsureCreated for schema initialization. During execution, the correct approach turned out to be MigrateAsync due to multi-schema shared PostgreSQL behavior. Multiple production bugs were also discovered and auto-fixed.

### Auto-fixed Issues

**1. [Rule 1 - Bug] EnsureCreated skips when tables already exist in shared PostgreSQL DB**
- **Found during:** Task 1 (ApiWebApplicationFactory)
- **Issue:** EnsureCreated returns false without creating schema if ANY table exists in the database. Since all 8 DbContexts share one PostgreSQL database (one schema each), the first context (OutboxDbContext) creates its schema, then all others skip.
- **Fix:** Changed InitializeAsync to use MigrateAsync for all 8 DbContexts; MigrateAsync uses per-schema __EFMigrationsHistory tables independently
- **Files modified:** ApiWebApplicationFactory.cs
- **Verification:** All 29 integration tests pass with correct schema creation
- **Committed in:** 3172c8b9 (Task 1 commit)

**2. [Rule 1 - Bug] ReplaceDbContext missing UseSnakeCaseNamingConvention**
- **Found during:** Task 1 (ApiWebApplicationFactory)
- **Issue:** Production code applies snake_case naming via BaseDbContext but ReplaceDbContext helper did not, causing EF to generate PascalCase column names that don't match the migration schema
- **Fix:** Added options.UseSnakeCaseNamingConvention() to ReplaceDbContext helper
- **Files modified:** ApiWebApplicationFactory.cs
- **Verification:** Database queries succeed with correct column names
- **Committed in:** 3172c8b9 (Task 1 commit)

**3. [Rule 1 - Bug] MassTransit health check deduplication via PostConfigure**
- **Found during:** Task 1 (ApiWebApplicationFactory)
- **Issue:** Pre-removing MassTransit health check descriptors didn't work — AddMassTransitTestHarness re-adds them after removal, causing duplicate health check names
- **Fix:** Used PostConfigure<HealthCheckServiceOptions> to deduplicate by name after all registrations complete
- **Files modified:** ApiWebApplicationFactory.cs
- **Verification:** Factory starts without ArgumentException for duplicate health checks
- **Committed in:** 3172c8b9 (Task 1 commit)

**4. [Rule 1 - Bug] GetProducts returns 400 with no query params**
- **Found during:** Task 2 (running integration tests)
- **Issue:** [FromQuery] int page without default value causes Minimal API to return 400 Bad Request when params are omitted
- **Fix:** Moved ISender/CancellationToken first, added = 1 and = 20 defaults to page/pageSize
- **Files modified:** CatalogEndpoints.cs
- **Verification:** GetProducts_EmptyDatabase_ReturnsEmptyList passes (was failing with 400)
- **Committed in:** 4be777c2 (Task 2 commit)

**5. [Rule 1 - Bug] GetCategories LINQ translation failure for .OrderBy(c => c.Name.Value)**
- **Found during:** Task 2 (running integration tests)
- **Issue:** EF cannot translate .Name.Value sub-property access when Name uses HasConversion (value object with EF converter)
- **Fix:** Changed .OrderBy(c => c.Name.Value) to .OrderBy(c => c.Name) directly
- **Files modified:** GetCategoriesQueryHandler.cs
- **Verification:** GetCategories_ReturnsCategories passes
- **Committed in:** 4be777c2 (Task 2 commit)

**6. [Rule 1 - Bug] GetOrderDashboard DateTimeOffset timezone rejection by Npgsql**
- **Found during:** Task 2 (running integration tests)
- **Issue:** DateTimeOffset.UtcNow.Date returns DateTime with Kind=Local which gets implicit local timezone offset when converted to DateTimeOffset — Npgsql rejects non-UTC DateTimeOffset values
- **Fix:** Changed to new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date, TimeSpan.Zero) throughout GetOrderDashboardQueryHandler
- **Files modified:** GetOrderDashboardQueryHandler.cs
- **Verification:** GetDashboard_ReturnsDashboardStats passes
- **Committed in:** 4be777c2 (Task 2 commit)

**7. [Rule 1 - Bug] GetOrderDashboard GroupBy cannot be translated to SQL**
- **Found during:** Task 2 (running integration tests)
- **Issue:** EF/Npgsql cannot translate DateTimeOffset.Date + GroupBy + DateOnly.FromDateTime to SQL in this EF version
- **Fix:** Fetched CreatedAt timestamps as List<DateTimeOffset> then grouped in memory with Dictionary<DateOnly, int>
- **Files modified:** GetOrderDashboardQueryHandler.cs
- **Verification:** GetDashboard_ReturnsDashboardStats passes
- **Committed in:** 4be777c2 (Task 2 commit)

**8. [Rule 1 - Bug] StockItem.Reserve returns 400 instead of 409 for insufficient stock**
- **Found during:** Task 2 (running integration tests)
- **Issue:** Reserve() threw InvalidOperationException (mapped to HTTP 400) but test expected 409 Conflict; insufficient stock is semantically a conflict
- **Fix:** Changed to throw ConflictException (mapped to HTTP 409 by exception middleware)
- **Files modified:** StockItem.cs, StockItemTests.cs
- **Verification:** ReserveStock_InsufficientStock_Returns409 passes
- **Committed in:** 4be777c2 (Task 2 commit)

**9. [Rule 1 - Bug] OrderStatus not deserializable on client side**
- **Found during:** Task 2 (running integration tests)
- **Issue:** HttpClient GetFromJsonAsync doesn't apply SmartEnum converter — orders returned with null/invalid status
- **Fix:** Added [JsonConverter(typeof(SmartEnumNameConverter<OrderStatus, int>))] attribute to OrderStatus class so it applies globally
- **Files modified:** OrderStatus.cs
- **Verification:** GetAllOrders_ReturnsOrderList passes
- **Committed in:** 4be777c2 (Task 2 commit)

**10. [Rule 1 - Bug] 9 pre-existing OrderTests error message patterns did not match TransitionTo format**
- **Found during:** Task 2 (running unit tests after integration test fixes)
- **Issue:** Unit tests asserted old format like "Cannot ship order when status is 'Paid'" but OrderStatus.TransitionTo throws "Cannot transition from 'Paid' to 'Shipped'. Valid transitions from 'Paid': ..."
- **Fix:** Updated 9 WithMessage patterns to match actual TransitionTo format: "*Cannot transition from 'X' to 'Y'*"
- **Files modified:** OrderTests.cs
- **Verification:** All 144 unit tests pass
- **Committed in:** 4be777c2 (Task 2 commit)

---

**Total deviations:** 10 auto-fixed (9 Rule 1 bugs, 1 encompassing 9 sub-fixes)
**Impact on plan:** All auto-fixes were required for tests to pass. Production fixes (GetProducts params, OrderBy translation, DateTimeOffset UTC, GroupBy in-memory, ConflictException, SmartEnum JsonConverter) correct real bugs. Test fixes (OrderTests messages, CatalogEndpointsTests isolation) fix pre-existing test errors. No scope creep.

## Issues Encountered

- EF1002 warning on ExecuteSqlRawAsync with schema name from EF model metadata: suppressed with #pragma warning disable EF1002 since schema name is not user input and TreatWarningsAsErrors would otherwise fail the build
- CatalogEndpointsTests isolation: GetProducts_EmptyDatabase_ReturnsEmptyList fails when other catalog tests run first due to shared DB state; fixed by extending IntegrationTestBase and calling ResetDatabase(typeof(CatalogDbContext)) in InitializeAsync
- UpdateOrderStatus test expected 204 or 400 but Result pattern endpoint returns 422 UnprocessableEntity for business rule violations (documented in Phase 17-02 decisions); test updated to also accept 422

## Next Phase Readiness

- Integration test infrastructure is solid: all 29 existing tests pass, per-schema isolation works
- IntegrationTestBase provides authenticated/guest client and schema reset helpers for Phase 20-02
- FakeAuthenticationHandler enables testing authenticated endpoints without Keycloak
- Ready for Phase 20-02: integration tests for authenticated user flows (cart, ordering, wishlist, profile)

---
*Phase: 20-integration-testing-infrastructure*
*Completed: 2026-02-25*
