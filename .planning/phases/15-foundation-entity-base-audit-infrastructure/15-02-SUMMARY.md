---
phase: 15-foundation-entity-base-audit-infrastructure
plan: 02
subsystem: ApiService.Common.Persistence
tags: [ef-core, interceptors, audit, concurrency, soft-delete, global-exception-handling]
dependency_graph:
  requires:
    - 15-01 (Entity base classes and marker interfaces)
  provides:
    - AuditInterceptor for IAuditable timestamp automation
    - ConcurrencyInterceptor for IConcurrencyToken versioning
    - SoftDeleteInterceptor for ISoftDeletable deletion conversion
    - SoftDeleteQueryFilterConvention for global query filters
    - ConcurrencyTokenConvention for EF Core concurrency token marking
    - DbUpdateConcurrencyException to HTTP 409 mapping
  affects:
    - All DbContexts when interceptors are wired (Phase 21)
    - All HTTP responses when concurrency conflicts occur
tech_stack:
  added: []
  patterns:
    - EF Core SaveChangesInterceptor for cross-cutting concerns
    - ModelBuilder conventions for configuration by interface
    - Expression-based query filters for soft delete filtering
key_files:
  created:
    - src/MicroCommerce.ApiService/Common/Persistence/AuditInterceptor.cs
    - src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyInterceptor.cs
    - src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteInterceptor.cs
    - src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteQueryFilterConvention.cs
    - src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyTokenConvention.cs
  modified:
    - src/MicroCommerce.ApiService/Common/Exceptions/GlobalExceptionHandler.cs
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - Interceptor order: SoftDelete, Concurrency, Audit, DomainEvent (critical for correct behavior)
  - SoftDeleteInterceptor also updates IAuditable.UpdatedAt on soft delete (track deletion as modification)
  - DbUpdateConcurrencyException returns HTTP 409 with entity type name for debugging
  - Convention helpers return ModelBuilder for fluent chaining
  - Interceptors registered in DI but not wired to DbContexts yet (Phase 21 adoption)
metrics:
  duration: 2
  tasks_completed: 2
  files_created: 5
  files_modified: 2
  commits: 2
  completed_at: 2026-02-14T09:28:13Z
---

# Phase 15 Plan 02: EF Core Interceptors & Conventions Summary

**One-liner:** Created three EF Core SaveChangesInterceptors (Audit, Concurrency, SoftDelete) with ModelBuilder convention helpers and updated GlobalExceptionHandler to return HTTP 409 on concurrency conflicts with entity details.

## Objective Achieved

Built the complete infrastructure for automatic IAuditable timestamp management, IConcurrencyToken versioning, and ISoftDeletable soft-delete conversion. These interceptors operate on the marker interfaces from Plan 01, making cross-cutting concerns fully automated without polluting domain logic.

## Work Completed

### Task 1: Create AuditInterceptor, ConcurrencyInterceptor, and SoftDeleteInterceptor

**Created `AuditInterceptor.cs`:**
- Sealed class extending `SaveChangesInterceptor`
- Overrides both `SavingChanges` and `SavingChangesAsync` methods
- Helper method `UpdateAuditFields` captures `DateTimeOffset.UtcNow` once per save
- On `EntityState.Added`: sets both `CreatedAt = now` and `UpdatedAt = now`
- On `EntityState.Modified`: sets `UpdatedAt = now` only
- Iterates `ChangeTracker.Entries<IAuditable>()` to find all auditable entities

**Created `ConcurrencyInterceptor.cs`:**
- Sealed class extending `SaveChangesInterceptor`
- Overrides both sync and async `SavingChanges` methods
- Helper method `IncrementVersions` processes all `IConcurrencyToken` entities
- On `EntityState.Added`: sets `Version = 1`
- On `EntityState.Modified`: increments `Version++`
- Automatic versioning prevents manual tracking in domain code

**Created `SoftDeleteInterceptor.cs`:**
- Sealed class extending `SaveChangesInterceptor`
- Overrides both sync and async `SavingChanges` methods
- Helper method `ConvertDeletesToSoftDeletes` intercepts hard deletes
- On `EntityState.Deleted`:
  - Changes state to `EntityState.Modified`
  - Sets `IsDeleted = true`
  - Sets `DeletedAt = now`
  - If entity implements `IAuditable`, also sets `UpdatedAt = now` (deletion as modification)
- Prevents physical deletion while maintaining EF Core delete semantics in domain code

**Verification:** `dotnet build` compiled with zero errors. All three interceptors created in correct namespace.

**Commit:** `f6f5812d` - feat(15-02): create audit, concurrency, and soft-delete interceptors

### Task 2: Create Convention Helpers, Register Interceptors, and Add DbUpdateConcurrencyException Handling

**Created `SoftDeleteQueryFilterConvention.cs`:**
- Static class with extension method `ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)`
- Iterates `modelBuilder.Model.GetEntityTypes()` to find all entities
- For each entity implementing `ISoftDeletable`:
  - Builds expression filter: `e => !e.IsDeleted`
  - Uses `Expression.Parameter`, `Expression.Property`, `Expression.Not`, `Expression.Lambda`
  - Calls `entityType.SetQueryFilter(filter)` to apply global filter
- Automatically excludes soft-deleted entities from all EF queries
- Returns `ModelBuilder` for fluent chaining

**Created `ConcurrencyTokenConvention.cs`:**
- Static class with extension method `ApplyConcurrencyTokenConvention(this ModelBuilder modelBuilder)`
- Iterates all entity types implementing `IConcurrencyToken`
- Finds `Version` property via `entityType.FindProperty(nameof(IConcurrencyToken.Version))`
- Sets `versionProperty.IsConcurrencyToken = true`
- Enables EF Core optimistic concurrency checks on `Version` column

**Updated `GlobalExceptionHandler.cs`:**
- Added `using Microsoft.EntityFrameworkCore;`
- Added new case in exception switch: `DbUpdateConcurrencyException dbConcurrencyEx`
- Returns `StatusCodes.Status409Conflict` with title "Concurrency Conflict"
- Detail message: `"The resource was modified by another request. {FormatConcurrencyDetail(dbConcurrencyEx)}"`
- Helper method `FormatConcurrencyDetail` extracts entity type name from `ex.Entries.FirstOrDefault()`
- Returns format: `"Entity: {EntityTypeName}. Please refresh and retry."` or fallback `"Please refresh and retry."`
- Provides actionable error info for frontend retry logic

**Updated `Program.cs`:**
- Added 3 interceptor registrations after existing `AddScoped<DomainEventInterceptor>()` (line 144):
  - `builder.Services.AddScoped<SoftDeleteInterceptor>();`
  - `builder.Services.AddScoped<ConcurrencyInterceptor>();`
  - `builder.Services.AddScoped<AuditInterceptor>();`
- Registration order documented: SoftDelete → Concurrency → Audit → DomainEvent
- Critical ordering: SoftDelete runs first to change Deleted→Modified, then Concurrency increments version on Modified, then Audit sets timestamps on Modified/Added
- Interceptors registered but NOT wired to DbContexts (Phase 21 scope)

**Verification:**
- `dotnet build` compiled with zero errors, zero warnings (only pre-existing package vulnerability warnings)
- `dotnet test --filter FullyQualifiedName~Unit` passed all 144 unit tests with zero regressions
- Integration tests failed due to Docker unavailability (infrastructure issue, not code regression)

**Commit:** `c6309bc8` - feat(15-02): add conventions, register interceptors, and handle concurrency exceptions

## Interceptor Execution Order

**Critical ordering (SavingChanges interceptors run in DI registration order):**

1. **SoftDeleteInterceptor** (first): Converts `EntityState.Deleted` → `EntityState.Modified`
2. **ConcurrencyInterceptor**: Increments `Version++` on `Modified` (includes soft deletes)
3. **AuditInterceptor**: Sets `UpdatedAt` on `Modified` (includes soft deletes and updates)
4. **DomainEventInterceptor** (existing, SavedChangesAsync): Publishes domain events after save completes

This order ensures soft deletes trigger version increments and timestamp updates correctly.

## Deviations from Plan

None - plan executed exactly as written.

## Key Decisions

1. **Interceptor registration order**: Documented as SoftDelete → Concurrency → Audit → DomainEvent (critical for correct cross-cutting behavior)
2. **Soft delete updates audit timestamp**: SoftDeleteInterceptor also sets `IAuditable.UpdatedAt` when soft-deleting, treating deletion as a modification
3. **DbUpdateConcurrencyException details**: Includes entity type name in HTTP 409 response for frontend debugging
4. **Convention fluent API**: Both convention helpers return `ModelBuilder` for method chaining
5. **Deferred DbContext wiring**: Interceptors registered in DI but NOT added to DbContexts via `AddInterceptors` (Phase 21 scope when entities actually implement these interfaces)

## Files Created

1. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Persistence/AuditInterceptor.cs` - Auto-sets CreatedAt/UpdatedAt timestamps
2. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyInterceptor.cs` - Auto-increments Version for optimistic concurrency
3. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteInterceptor.cs` - Converts hard deletes to soft deletes
4. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteQueryFilterConvention.cs` - Global query filter for ISoftDeletable
5. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyTokenConvention.cs` - EF Core concurrency token marking

## Files Modified

1. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Common/Exceptions/GlobalExceptionHandler.cs` - Added DbUpdateConcurrencyException → HTTP 409 mapping
2. `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Program.cs` - Registered 3 new interceptors in DI

## Impact

**Immediate:**
- Infrastructure layer complete for audit, concurrency, and soft-delete automation
- HTTP 409 responses now provide entity type context for concurrency conflicts
- All unit tests pass (144/144) - zero regressions from changes

**Future (Phase 21 adoption):**
- DbContexts will wire interceptors via `AddInterceptors` in `OnConfiguring`
- Entities implementing `IAuditable` will have timestamps auto-populated
- Entities implementing `IConcurrencyToken` will have version auto-incremented
- Entities implementing `ISoftDeletable` will never be physically deleted
- Global query filters will automatically exclude soft-deleted entities from queries
- Concurrency conflicts will return HTTP 409 with actionable error messages

## Next Steps

Phase 21 will migrate existing aggregates to implement these marker interfaces and wire interceptors into DbContexts. This completes Phase 15 foundation work.

## Self-Check: PASSED

**Created files verified:**
```
FOUND: src/MicroCommerce.ApiService/Common/Persistence/AuditInterceptor.cs
FOUND: src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyInterceptor.cs
FOUND: src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteInterceptor.cs
FOUND: src/MicroCommerce.ApiService/Common/Persistence/SoftDeleteQueryFilterConvention.cs
FOUND: src/MicroCommerce.ApiService/Common/Persistence/ConcurrencyTokenConvention.cs
```

**Modified files verified:**
```
FOUND: src/MicroCommerce.ApiService/Common/Exceptions/GlobalExceptionHandler.cs (DbUpdateConcurrencyException handling)
FOUND: src/MicroCommerce.ApiService/Program.cs (interceptor DI registration)
```

**Commits verified:**
```
FOUND: f6f5812d (Task 1: Three SaveChangesInterceptors)
FOUND: c6309bc8 (Task 2: Conventions, DI registration, exception handling)
```

**Build verification:**
- MicroCommerce.ApiService: 0 errors (2 pre-existing package vulnerability warnings unrelated to changes)
- Unit tests: 144/144 passed, 0 failures, 0 regressions
