# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-14)

**Core value:** A user can complete a purchase end-to-end
**Current focus:** Milestone v2.0 DDD Foundation — Phase 18 in progress (Plan 1 of 2 complete)

## Current Position

Phase: 18 of 21 (Enumeration — Enums with Behavior) — In Progress
Plan: 1 of 2 completed
Status: Plan 18-01 Complete — SmartEnum infrastructure (PRIM-02) in place
Last activity: 2026-02-24 — Completed 18-01: SmartEnum packages, OrderStatus/ProductStatus types, EF Core + JSON converters

Progress: [■■■■■■■■■■■■■■■■■■■■■■░░░░░░░░] 82% (80/96 plans completed)

## Performance Metrics

**Velocity:**
- Total plans completed: 77 (v1.0: 49, v1.1: 23, v2.0: 5)
- Average duration: 22 min
- Total execution time: 27.95 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 8 | 37 min | In progress |

**Recent Trend:**
- Last 5 plans: v2.0 foundation infrastructure (16.1-01, 16.1-02, 17-01, 17-02, 18-01)
- Trend: DDD foundation layer + EF Core conventions complete, Vogen adoption complete, FluentResults Result pattern complete (Phase 17), SmartEnum infrastructure complete (Phase 18 Plan 01)

*Updated after 18-01 completion*
| Phase 15 P02 | 2 | 2 tasks | 7 files |
| Phase 16-conventions-dry-configuration P01 | 7 | 2 tasks | 14 files |
| Phase 16 P02 | 13 | 2 tasks | 45 files |
| Phase 16.1-adopt-vogen-for-value-object P01 | 4 | 2 tasks | 43 files |
| Phase 16.1-adopt-vogen-for-value-object P02 | 4 | 1 task | 16 files |
| Phase 17-result-pattern P01 | 3 | 2 tasks | 5 files |
| Phase 17-result-pattern P02 | 4 | 2 tasks | 6 files |
| Phase 18-enumeration-enums-with-behavior P01 | 3 | 2 tasks | 13 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Recent decisions affecting v2.0:
- **Readonly record structs for value objects**: 20x faster equality, adopted in v1.1
- **Vernon's aggregate rules**: Audit standard for DDD compliance
- **Reference by identity only**: Proper aggregate isolation enforced
- **Entity reference equality** (15-01): Entity<TId> uses reference equality only, no custom Equals/GetHashCode
- **Settable marker interfaces** (15-01): IAuditable/IConcurrencyToken/ISoftDeletable use { get; set; } for interceptor modification
- **IAuditable timestamps only** (15-01): CreatedAt/UpdatedAt only for now, extensible to user tracking later
- **IConcurrencyToken uses int** (15-01): int Version for portability and simplicity over uint
- [Phase 15-02]: Interceptor order: SoftDelete, Concurrency, Audit, DomainEvent (critical for correct behavior)
- [Phase 15-02]: SoftDeleteInterceptor also updates IAuditable.UpdatedAt on soft delete
- [Phase 15-02]: DbUpdateConcurrencyException returns HTTP 409 with entity type name
- [Phase 16-01]: IModelFinalizingConvention over static ModelBuilder extension methods for proper EF Core convention API
- [Phase 16-01]: BaseDbContext abstract class as single registration point for all EF Core conventions and snake_case naming
- [Phase 16]: StronglyTypedIdConvention fix: use typed generic method CreateTypedConverter<TId,TUnderlying> for ValueConverter constructor compatibility
- [Phase 16]: UseSnakeCaseNamingConvention moved to Program.cs configureDbContextOptions (not OnConfiguring) for DbContext pooling compatibility
- [Phase 16]: CheckoutState IsRowVersion and ToTable kept explicit (MassTransit saga - not IConcurrencyToken)
- [Phase 16.1-01]: Vogen 8.0.4 partial record structs with [ValueObject<Guid>] replace hand-rolled StronglyTypedId<T> class hierarchy
- [Phase 16.1-01]: VogenEfCoreConverters marker class with [EfCoreConverter<T>] attributes replaces reflection-based StronglyTypedIdConvention
- [Phase 16.1-01]: EventId uses Conversions.SystemTextJson only (no EfCoreValueConverter) as it is not an EF entity property
- [Phase 16.1-02]: VogenIds migrations have empty Up()/Down() for all 8 contexts — RegisterAllInVogenEfCoreConverters() is applied at runtime via ConfigureConventions and is not recorded in model snapshots
- [Phase 16.1-02]: Model snapshots unchanged after Vogen adoption — convention-based converters have no static snapshot representation
- [Phase 17-result-pattern]: FluentResults 4.0.0 in BuildingBlocks.Common, ResultValidationBehavior with IResultBase constraint, ToHttpResult maps success to 204 and failure to 422, Result vs Exception boundary documented in ADR-006
- [Phase 17-02]: Domain methods keep throwing InvalidOperationException — handlers catch and convert to Result.Fail (adapter pattern at handler boundary)
- [Phase 17-02]: UpdateOrderStatus/AdjustStock now return 422 Unprocessable Entity for business rule violations instead of 500 from uncaught InvalidOperationException
- [Phase 18-01]: SmartEnumStringConverter stores by Name (not Value) to preserve existing string-based DB schema — do NOT use Ardalis.SmartEnum.EFCore ConfigureSmartEnum() which stores by int Value
- [Phase 18-01]: Per-type HaveConversion<SmartEnumStringConverter<T>>() registration in BaseDbContext ConfigureConventions (simpler than IModelFinalizingConvention for 2 types)
- [Phase 18-01]: SmartEnum types are abstract (not sealed) to allow subclassing per CONTEXT.md
- [Phase 18-01]: HasConversion<string>() removed from entity configurations — base convention handles it

### Roadmap Evolution

- v1.0 MVP: Phases 1-10 (shipped 2026-02-13)
- v1.1 User Features: Phases 11-14.3 (shipped 2026-02-14)
- v2.0 DDD Foundation: Phases 15-21 (current, roadmap created 2026-02-14)
- Phase 16.1 inserted after Phase 16: Adopt Vogen for value object (URGENT)

### Pending Todos

None.

### Blockers/Concerns

None. All v1.1 issues resolved. Clean slate for v2.0.

## Session Continuity

Last session: 2026-02-24
Stopped at: Completed 18-01-PLAN.md (SmartEnum infrastructure — OrderStatus/ProductStatus types, EF Core + JSON converters)
Resume file: None
Next step: Phase 18 Plan 02 — migrate entity methods and query handlers to use SmartEnum transition rules
