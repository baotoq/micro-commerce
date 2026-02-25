# Phase 10: Testing & Polish - Context

**Gathered:** 2026-02-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Comprehensive testing coverage for the MicroCommerce platform and production-readiness polish. Unit tests for domain logic, integration tests for API endpoints and saga flows, E2E test for the critical purchase path, and UX polish pass across storefront and admin. No new features — this phase hardens what's already built.

</domain>

<decisions>
## Implementation Decisions

### Test scope & priorities
- Ordering & Checkout gets the most thorough unit test coverage (highest complexity/risk)
- Other domains (Catalog, Inventory, Cart) get proportional coverage but Ordering is priority
- Backend only — no React component tests in this phase (frontend tested via E2E)
- FluentValidation validators get dedicated test classes (not just tested through integration tests)

### Integration test strategy
- Testcontainers with real PostgreSQL for database tests — high fidelity, catches DB-specific issues
- MassTransit InMemoryTestHarness for message bus tests — fast, verifies consumer logic without real bus
- Checkout saga tested via saga state machine unit tests (MassTransit SagaTestHarness)
- Test project: `src/MicroCommerce.ApiService.Tests/` using xUnit + WebApplicationFactory

### E2E test approach
- Tool choice, journey coverage, and organization at Claude's discretion
- Must cover at minimum the critical path: browse → cart → checkout → order confirmation

### Polish & UX gaps
- Claude audits and fixes the most impactful UX gaps across storefront and admin
- Accessibility improvements skipped for this phase
- Responsive design and error handling at Claude's discretion (fix obvious issues)
- No specific known issues flagged — Claude performs audit to identify gaps

### Claude's Discretion
- E2E tool choice (Playwright vs Cypress vs other)
- E2E journey selection beyond happy path
- Integration test organization (per-endpoint vs per-workflow)
- Unit test granularity per aggregate (public API only vs internal guards)
- Which polish areas to prioritize after audit
- Responsive breakpoint fixes
- Error handling improvements scope

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches. The project is a showcase platform, so tests should demonstrate good testing patterns for a modular monolith with CQRS, DDD, and event-driven architecture.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 10-testing-polish*
*Context gathered: 2026-02-12*
