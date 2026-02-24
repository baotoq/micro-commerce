# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-14)

**Core value:** A user can complete a purchase end-to-end
**Current focus:** Milestone v2.0 DDD Foundation — Phase 16: Conventions & DRY Configuration

## Current Position

Phase: 16 of 21 (Conventions DRY Configuration)
Plan: 1 of 2 completed
Status: In Progress
Last activity: 2026-02-24 — Completed 16-01: EF Core Conventions and BaseDbContext

Progress: [■■■■■■■■■■■■■■■■■■■■░░░░░░░░░░] 74% (75/96 plans completed)

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
| v2.0 DDD Foundation | 7 | 2 | 3 min | In progress |

**Recent Trend:**
- Last 5 plans: v2.0 foundation infrastructure (15-01, 15-02, 16-01)
- Trend: DDD foundation layer + EF Core conventions complete

*Updated after 15-02 completion*
| Phase 15 P02 | 2 | 2 tasks | 7 files |
| Phase 16-conventions-dry-configuration P01 | 7 | 2 tasks | 14 files |

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

### Roadmap Evolution

- v1.0 MVP: Phases 1-10 (shipped 2026-02-13)
- v1.1 User Features: Phases 11-14.3 (shipped 2026-02-14)
- v2.0 DDD Foundation: Phases 15-21 (current, roadmap created 2026-02-14)

### Pending Todos

None.

### Blockers/Concerns

None. All v1.1 issues resolved. Clean slate for v2.0.

## Session Continuity

Last session: 2026-02-24
Stopped at: Completed 16-01-PLAN.md (EF Core conventions and BaseDbContext)
Resume file: None
Next step: Phase 16 Plan 02 — DRY configuration cleanup (remove manual HasConversion calls from entity configurations)
