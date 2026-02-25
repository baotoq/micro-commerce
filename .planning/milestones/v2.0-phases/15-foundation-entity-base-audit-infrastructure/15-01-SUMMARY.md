---
phase: 15-foundation-entity-base-audit-infrastructure
plan: 01
subsystem: BuildingBlocks.Common
tags: [ddd, entity-hierarchy, audit, concurrency, soft-delete]
dependency_graph:
  requires: []
  provides:
    - Entity<TId> base class for child entities
    - BaseAggregateRoot<TId> extending Entity<TId>
    - IAuditable interface for audit timestamps
    - IConcurrencyToken interface for optimistic concurrency
    - ISoftDeletable interface for soft deletion
    - AuditableAggregateRoot<TId> convenience base class
  affects:
    - All future aggregate roots and entities
    - Phase 21 entity migrations
tech_stack:
  added: []
  patterns:
    - DDD entity hierarchy with typed IDs
    - Marker interfaces for cross-cutting concerns
    - Reference equality for entities (no custom Equals/GetHashCode)
key_files:
  created:
    - src/BuildingBlocks/BuildingBlocks.Common/Entity.cs
    - src/BuildingBlocks/BuildingBlocks.Common/IAuditable.cs
    - src/BuildingBlocks/BuildingBlocks.Common/IConcurrencyToken.cs
    - src/BuildingBlocks/BuildingBlocks.Common/ISoftDeletable.cs
    - src/BuildingBlocks/BuildingBlocks.Common/AuditableAggregateRoot.cs
  modified:
    - src/BuildingBlocks/BuildingBlocks.Common/BaseAggregateRoot.cs
decisions:
  - Use reference equality for Entity<TId> (no custom Equals/GetHashCode override)
  - IAuditable has only timestamps (no CreatedBy/ModifiedBy yet, extensible later)
  - IConcurrencyToken uses int Version (portable, simple, human-readable)
  - ISoftDeletable uses DateTimeOffset? DeletedAt (nullable, only set when deleted)
  - All marker interfaces use settable properties for interceptor modification
metrics:
  duration: 1
  tasks_completed: 2
  files_created: 5
  files_modified: 1
  commits: 2
  completed_at: 2026-02-14T09:23:20Z
---

# Phase 15 Plan 01: Entity Base & Audit Infrastructure Summary

**One-liner:** Established foundational DDD entity hierarchy with Entity<TId> base class, refactored BaseAggregateRoot to extend it, and created IAuditable/IConcurrencyToken/ISoftDeletable marker interfaces with AuditableAggregateRoot convenience class.

## Objective Achieved

Created the complete DDD entity hierarchy and marker interfaces in BuildingBlocks.Common that all Phase 21 entity migrations will build upon. These types define the contracts for interceptors (Plan 02) to operate on.

## Work Completed

### Task 1: Create Entity Base Class and Refactor BaseAggregateRoot

**Created `Entity.cs`:**
- Abstract base class `Entity<TId>` with typed `Id` property
- Uses reference equality (no custom Equals/GetHashCode per user decision)
- Two protected constructors: parameterless and `Entity(TId id)`
- Provides identity foundation for all child entities

**Refactored `BaseAggregateRoot.cs`:**
- Changed from primary constructor to regular class extending `Entity<TId>`
- Removed duplicate `Id` property (now inherited from Entity)
- Maintains backward compatibility - all existing aggregates compile without changes
- Kept domain event infrastructure unchanged: `_domainEvents`, `AddDomainEvent`, `DomainEvents`, `ClearDomainEvents`

**Verification:** Both BuildingBlocks.Common and ApiService built with zero errors, confirming backward compatibility.

**Commit:** `4850fd79` - feat(15-01): create Entity base class and refactor BaseAggregateRoot

### Task 2: Create Marker Interfaces and AuditableAggregateRoot

**Created `IAuditable.cs`:**
- Interface with `CreatedAt` and `UpdatedAt` as `DateTimeOffset` properties
- Settable (not init-only) for interceptor modification
- Timestamps only for now (extensible to CreatedBy/ModifiedBy later)

**Created `IConcurrencyToken.cs`:**
- Interface with single `int Version` property
- Settable for auto-increment by interceptor
- Uses int (not uint) for portability and simplicity

**Created `ISoftDeletable.cs`:**
- Interface with `bool IsDeleted` and `DateTimeOffset? DeletedAt`
- DeletedAt is nullable, only set when soft-deleted

**Created `AuditableAggregateRoot.cs`:**
- Abstract class extending `BaseAggregateRoot<TId>` and implementing `IAuditable`
- Convenience base for aggregates needing audit timestamps
- Two protected constructors for parameterless and ID-based initialization

**Verification:** BuildingBlocks.Common built cleanly with all 4 new types.

**Commit:** `daa32ff6` - feat(15-01): add audit, concurrency, and soft-delete marker interfaces

## Type Hierarchy Established

```
Entity<TId> (base for all entities)
  └─ BaseAggregateRoot<TId> : Entity<TId>, IAggregateRoot
       └─ AuditableAggregateRoot<TId> : BaseAggregateRoot<TId>, IAuditable

Orthogonal marker interfaces:
- IAuditable (CreatedAt, UpdatedAt)
- IConcurrencyToken (Version)
- ISoftDeletable (IsDeleted, DeletedAt)
```

## Deviations from Plan

None - plan executed exactly as written.

## Key Decisions

1. **Reference equality for Entity<TId>**: No custom Equals/GetHashCode override, following user's established pattern from previous decisions
2. **IAuditable scope**: Timestamps only (CreatedAt/UpdatedAt), no user tracking yet - extensible for future needs
3. **IConcurrencyToken type**: Uses `int` Version instead of `uint` for portability, simplicity, and human readability
4. **ISoftDeletable nullable DeletedAt**: Only populated when entity is deleted, null otherwise
5. **Settable interface properties**: All marker interfaces use `{ get; set; }` to enable interceptor modification in Plan 02

## Files Created

1. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/Entity.cs` - Base entity class with typed identity
2. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/IAuditable.cs` - Audit timestamp interface
3. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/IConcurrencyToken.cs` - Concurrency token interface
4. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/ISoftDeletable.cs` - Soft delete marker interface
5. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/AuditableAggregateRoot.cs` - Convenience auditable base class

## Files Modified

1. `/Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/BaseAggregateRoot.cs` - Refactored to extend Entity<TId>

## Impact

**Immediate:**
- BuildingBlocks.Common now provides complete DDD entity foundation
- All existing aggregates continue to work (backward compatible)
- Type hierarchy ready for Phase 21 migrations

**Future:**
- Phase 21 will migrate existing aggregates to use Entity<TId> hierarchy
- Plan 02 will create interceptors operating on these marker interfaces
- IAuditable/IConcurrencyToken/ISoftDeletable provide extension points for cross-cutting concerns

## Next Steps

Execute Plan 02 to create EF Core interceptors that automatically populate IAuditable timestamps, increment IConcurrencyToken versions, and apply global query filters for ISoftDeletable entities.

## Self-Check: PASSED

**Created files verified:**
```
FOUND: src/BuildingBlocks/BuildingBlocks.Common/Entity.cs
FOUND: src/BuildingBlocks/BuildingBlocks.Common/IAuditable.cs
FOUND: src/BuildingBlocks/BuildingBlocks.Common/IConcurrencyToken.cs
FOUND: src/BuildingBlocks/BuildingBlocks.Common/ISoftDeletable.cs
FOUND: src/BuildingBlocks/BuildingBlocks.Common/AuditableAggregateRoot.cs
```

**Commits verified:**
```
FOUND: 4850fd79 (Task 1: Entity base class and BaseAggregateRoot refactor)
FOUND: daa32ff6 (Task 2: Marker interfaces and AuditableAggregateRoot)
```

**Build verification:**
- BuildingBlocks.Common: 0 errors, 0 warnings
- MicroCommerce.ApiService: 0 errors (4 pre-existing package vulnerability warnings unrelated to changes)
