# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-14)

**Core value:** A user can complete a purchase end-to-end
**Current focus:** Milestone v2.0 DDD Foundation — Phase 17 (next)

## Current Position

Phase: 16.1 of 21 (Adopt Vogen for Value Object) — COMPLETE
Plan: 1 of 1 completed
Status: Phase 16.1 Complete
Last activity: 2026-02-24 — Completed 16.1-01: Vogen adoption for all 14 strongly-typed ID types

Progress: [■■■■■■■■■■■■■■■■■■■■■░░░░░░░░░] 77% (77/96 plans completed)

## Performance Metrics

**Velocity:**
- Total plans completed: 74 (v1.0: 49, v1.1: 23, v2.0: 2)
- Average duration: 22 min
- Total execution time: 27.7 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 4 | 23 min | In progress |

**Recent Trend:**
- Last 5 plans: v2.0 foundation infrastructure (15-01, 15-02, 16-01, 16-02)
- Trend: DDD foundation layer + EF Core conventions complete, configuration DRY complete

*Updated after 15-02 completion*
| Phase 15 P02 | 2 | 2 tasks | 7 files |
| Phase 16-conventions-dry-configuration P01 | 7 | 2 tasks | 14 files |
| Phase 16 P02 | 13 | 2 tasks | 45 files |
| Phase 16.1-adopt-vogen-for-value-object P01 | 4 | 2 tasks | 43 files |

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
Stopped at: Completed 16.1-01-PLAN.md (Vogen adoption for all 14 strongly-typed ID types)
Resume file: None
Next step: Phase 17 — next phase in v2.0 DDD Foundation milestone
