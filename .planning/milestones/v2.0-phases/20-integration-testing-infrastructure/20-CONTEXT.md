# Phase 20: Integration Testing Infrastructure - Context

**Gathered:** 2026-02-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Set up integration testing infrastructure using xUnit, WebApplicationFactory, and Testcontainers (PostgreSQL). Create one representative integration test per feature (7 features) to prove the pattern. This phase builds the test foundation that Phase 21's full adoption relies on. Existing unit tests remain untouched.

</domain>

<decisions>
## Implementation Decisions

### Test scope & pilots
- One representative integration test per feature (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists) — breadth over depth
- Two test layers: API endpoint tests (full HTTP pipeline) for happy paths, handler-level MediatR tests for edge cases and business rules
- Domain events: only assert that events are added to the aggregate's domain events collection — no MassTransit consumer/messaging tests
- New integration tests only — existing unit tests stay as-is in their current location

### Database isolation
- Single PostgreSQL Testcontainer for the entire test run, each test class gets a fresh database — fast startup, good isolation
- Per-test setup methods for data — each test arranges its own data using builders, no shared seed data
- EnsureCreated for schema creation (not migrations) — faster, sufficient for test validation
- Feature-scoped DbContext access — Catalog tests only touch CatalogDbContext, cross-feature tests compose what they need

### Test organization
- Same test project: add to existing `MicroCommerce.ApiService.Tests` with folder separation (`Integration/` alongside existing `Unit/`)
- Mirror feature folders: `Integration/Catalog/`, `Integration/Cart/`, `Integration/Ordering/`, etc.
- Custom `MicroCommerceWebAppFactory` (WebApplicationFactory subclass) configures Testcontainers + service overrides
- `IntegrationTestBase` base class provides helpers: HttpClient creation, DB access, seeding shortcuts
- Test data builder pattern: `ProductBuilder`, `OrderBuilder`, etc. with fluent API for readable, reusable test data

### Auth & identity in tests
- Fake authentication handler that bypasses Keycloak — tests set claims directly, no external auth dependency
- Helper methods on IntegrationTestBase: `CreateAuthenticatedClient(userId)`, `CreateGuestClient()` — test controls identity per request
- No authorization boundary tests (403/401) in this phase — focus on functional correctness
- Guest cart: set buyer identity cookie header directly on HttpClient — explicit, no cookie jar management

### Claude's Discretion
- Exact Testcontainers configuration and lifecycle management
- WebApplicationFactory service override details (connection strings, external service mocking)
- Builder pattern implementation details (fluent API design, default values)
- Which specific endpoint/handler to test per feature (pick the most representative one)

</decisions>

<specifics>
## Specific Ideas

- Domain event assertions should verify events are in the aggregate's DomainEvents collection, not that they were published/consumed
- IntegrationTestBase should make it trivial to write a new test — minimal boilerplate per test class

</specifics>

<deferred>
## Deferred Ideas

- MOD-04: OpenAPI schema filters for StronglyTypedId (primitive display) and SmartEnum (string display) — fold into Phase 21 or separate phase
- Keycloak Testcontainer for full auth fidelity testing — future enhancement
- Authorization boundary tests (role-based 403/401) — future enhancement
- MassTransit messaging integration tests (consumer handling) — future enhancement
- Frontend React Testing Library tests — separate concern

</deferred>

---

*Phase: 20-integration-testing-infrastructure*
*Context gathered: 2026-02-25*
