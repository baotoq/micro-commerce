# Phase 21: Adoption - Full Building Block Integration - Context

**Gathered:** 2026-02-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Apply all building blocks created in Phases 15-20 (Entity base, IAuditable, IConcurrencyToken, AuditableAggregateRoot, source-generated StronglyTypedId, Result pattern, Specification pattern) across all 7 feature modules (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists). Add OpenAPI schema filters for StronglyTypedId and SmartEnum display. Ensure all 180+ existing tests pass with no regressions.

</domain>

<decisions>
## Implementation Decisions

### Result pattern expansion
- Claude selects which additional command handlers (beyond the Phase 17 pilot: UpdateOrderStatus, AdjustStock) to migrate to Result pattern
- Selection criteria: handlers with meaningful business rule validation that can fail for domain reasons, not just input validation
- Endpoint mapping: use specific HTTP status codes for different failure types (409 Conflict for state violations, 422 for business rule failures, 404 for not found)
- Non-migrated handlers: leave as-is with no TODO comments — ADOPT-F01 already tracks full adoption as a future requirement

### Claude's Discretion
- Which specific command handlers to migrate to Result (2+ additional beyond pilot)
- Whether to add integration tests for newly-migrated Result handlers
- Specification pattern coverage — which queries beyond Phase 19 specs should use Specification
- Audit and concurrency scope — which aggregates get AuditableAggregateRoot, whether any entities beyond Order/Cart/StockItem need concurrency tokens
- OpenAPI schema filter design for StronglyTypedId (primitive display) and SmartEnum (string display)
- Migration sequencing (module-by-module vs building-block-by-building-block)

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches. User trusts Claude to make pragmatic decisions across all adoption areas, with the key constraint that Result pattern failures must map to specific HTTP status codes rather than generic 400s.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 21-adoption-full-building-block-integration*
*Context gathered: 2026-02-25*
