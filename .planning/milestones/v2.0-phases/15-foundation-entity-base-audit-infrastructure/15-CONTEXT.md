# Phase 15: Foundation - Entity Base & Audit Infrastructure - Context

**Gathered:** 2026-02-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Build the DDD building block infrastructure: Entity<TId> base class for child entities, IAuditable interface with auto-timestamp interceptor, AuditableAggregateRoot convenience base, IConcurrencyToken with explicit Version column, and ISoftDeletable with global query filter and interceptor. This phase builds the infrastructure only — full adoption across all 7 modules is Phase 21.

</domain>

<decisions>
## Implementation Decisions

### Entity base contract
- Reference equality only — do NOT override Equals/GetHashCode based on ID
- Default ToString — no custom override
- Abstract class (not record) — consistent with existing BaseAggregateRoot<TId> pattern

### Entity hierarchy
- Claude's discretion on whether BaseAggregateRoot<TId> extends Entity<TId> or stays separate — pick what reduces duplication while staying clean

### Concurrency version strategy
- Auto-increment via interceptor on SaveChanges — entities don't manage Version manually
- HTTP 409 Conflict with details (entity type + version info) on DbUpdateConcurrencyException — client knows to re-fetch and retry
- Scope: Only migrate the 7 entities already using xmin (Cart, Order, StockItem, Review, UserProfile, Wishlist, CheckoutState) — don't expand to Product/Category yet

### Claude's Discretion
- Entity hierarchy design (Entity as base for AggregateRoot, or separate)
- Version column type (int counter vs Guid vs byte[])
- Soft delete cascade behavior and implementation details
- Audit interceptor design (timestamp precision, UpdatedAt on soft-delete)
- IAuditable interface shape (accommodate future CreatedBy/ModifiedBy or not)

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches. The user wants clean, minimal infrastructure that follows established DDD patterns without over-engineering.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 15-foundation-entity-base-audit-infrastructure*
*Context gathered: 2026-02-14*
