# Phase 16: Conventions - DRY Configuration - Context

**Gathered:** 2026-02-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Eliminate repetitive manual EF Core configuration through model conventions. StronglyTypedId types auto-converted, IConcurrencyToken auto-configured, ValueObject base class removed, and entity configuration files simplified across all DbContexts.

</domain>

<decisions>
## Implementation Decisions

### Convention scope
- Conventions for StronglyTypedId value converters (auto HasConversion)
- Conventions for IConcurrencyToken (auto IsConcurrencyToken configuration)
- Conventions for ISoftDeletable (auto HasQueryFilter for soft-delete)
- Conventions for IAuditable (auto-configure CreatedAt/UpdatedAt column types and defaults)
- snake_case naming convention for all PostgreSQL tables and columns

### Configuration cleanup
- Remove redundant manual config lines that conventions now handle — single source of truth
- Delete entity configuration files that become empty or near-empty after cleanup
- Register conventions in a shared base class or helper that all DbContexts inherit/call — one place, automatic for all
- Individual DbContexts can override convention defaults for entity-specific cases when needed

### ValueObject removal
- Full cleanup: remove ValueObject base class AND all related infrastructure (converters, comparers, helpers)
- Migrate any remaining ValueObject inheritors to readonly record structs (v1.1 pattern)
- Generate EF Core migrations for any schema changes resulting from the cleanup
- Verify zero ValueObject references remain in codebase after cleanup (build + grep)

### Claude's Discretion
- Exact convention implementation approach (IModelFinalizingConvention vs ConfigureConventions)
- Order of convention registration
- How to structure the shared base class / helper for convention registration
- Migration strategy for snake_case (whether to rename existing tables/columns or apply to new only)

</decisions>

<specifics>
## Specific Ideas

- snake_case naming matches PostgreSQL conventions — use EFCore.NamingConventions or similar approach
- Shared convention registration should be DRY but allow per-DbContext overrides
- ValueObject removal should be thorough — no dead code left behind

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 16-conventions-dry-configuration*
*Context gathered: 2026-02-14*
