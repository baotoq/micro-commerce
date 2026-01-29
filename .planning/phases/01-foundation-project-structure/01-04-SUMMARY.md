---
phase: 01-foundation-project-structure
plan: 04
subsystem: infra
tags: [masstransit, azure-service-bus, outbox, domain-events, ef-core]

# Dependency graph
requires:
  - phase: 01-01
    provides: MassTransit NuGet packages and Aspire Service Bus resource
  - phase: 01-02
    provides: Module DbContexts (Catalog, Cart, Ordering, Inventory)
provides:
  - DomainEventInterceptor for automatic domain event publishing
  - OutboxDbContext for transactional outbox pattern
  - MassTransit Azure Service Bus configuration
  - Reliable at-least-once domain event delivery
affects: [phase-02, phase-05, phase-07]

# Tech tracking
tech-stack:
  added: [MassTransit outbox, Azure Service Bus transport]
  patterns: [transactional-outbox, save-changes-interceptor, domain-event-publishing]

key-files:
  created:
    - code/MicroCommerce.ApiService/Common/Persistence/DomainEventInterceptor.cs
    - code/MicroCommerce.ApiService/Common/Persistence/OutboxDbContext.cs
  modified:
    - code/MicroCommerce.ApiService/Program.cs
    - code/BuildingBlocks/BuildingBlocks.Common/Events/MediatorDomainEventDispatcher.cs
    - code/BuildingBlocks/BuildingBlocks.Common/Events/IDomainEventDispatcher.cs
    - code/BuildingBlocks/BuildingBlocks.Common/Events/IDomainEventHandler.cs
    - code/BuildingBlocks/BuildingBlocks.Common/DependencyInjection.cs

key-decisions:
  - "Use SaveChangesInterceptor.SavedChangesAsync for post-commit event dispatch"
  - "Outbox tables in dedicated 'outbox' schema"
  - "All module DbContexts registered with schema-specific migration history tables"

patterns-established:
  - "Transactional outbox: events saved in same transaction as aggregate changes"
  - "DomainEventInterceptor: automatic event collection from IAggregateRoot entities"
  - "Deprecation pattern: [Obsolete] with comprehensive migration documentation"

# Metrics
duration: 4min
completed: 2026-01-29
---

# Phase 01 Plan 04: Domain Event Infrastructure Summary

**MassTransit transactional outbox with Azure Service Bus for reliable domain event delivery, replacing in-process MediatR dispatch**

## Performance

- **Duration:** 4 min
- **Started:** 2026-01-29T15:35:45Z
- **Completed:** 2026-01-29T15:39:28Z
- **Tasks:** 3
- **Files modified:** 7

## Accomplishments

- Created OutboxDbContext with MassTransit outbox entities (InboxState, OutboxMessage, OutboxState) in dedicated 'outbox' schema
- Implemented DomainEventInterceptor (SaveChangesInterceptor) that publishes domain events via IPublishEndpoint after SaveChanges
- Configured MassTransit with Azure Service Bus transport and EF Core outbox pattern for at-least-once delivery
- Registered all module DbContexts (Catalog, Cart, Ordering, Inventory) with Aspire's AddNpgsqlDbContext
- Deprecated old in-process MediatorDomainEventDispatcher with comprehensive migration documentation

## Task Commits

Each task was committed atomically:

1. **Task 1: Create OutboxDbContext and DomainEventInterceptor** - `81c9fa0` (feat)
2. **Task 2: Configure MassTransit with Azure Service Bus and outbox** - `293f73d` (feat)
3. **Task 3: Deprecate old MediatorDomainEventDispatcher** - `f6f5c7e` (refactor)

## Files Created/Modified

- `code/MicroCommerce.ApiService/Common/Persistence/DomainEventInterceptor.cs` - EF Core interceptor that publishes domain events via MassTransit after SaveChanges
- `code/MicroCommerce.ApiService/Common/Persistence/OutboxDbContext.cs` - DbContext for MassTransit outbox tables in 'outbox' schema
- `code/MicroCommerce.ApiService/Program.cs` - MassTransit configuration, DbContext registration with Aspire
- `code/BuildingBlocks/BuildingBlocks.Common/Events/IDomainEventDispatcher.cs` - Marked obsolete with migration guidance
- `code/BuildingBlocks/BuildingBlocks.Common/Events/MediatorDomainEventDispatcher.cs` - Marked obsolete with MassTransit example
- `code/BuildingBlocks/BuildingBlocks.Common/Events/IDomainEventHandler.cs` - Marked obsolete, recommend IConsumer<T>
- `code/BuildingBlocks/BuildingBlocks.Common/DependencyInjection.cs` - Extension method marked obsolete

## Decisions Made

1. **SavedChangesAsync over SavingChanges:** Events are published AFTER SaveChanges completes (not during), ensuring database transaction is committed before event dispatch. This aligns with outbox pattern semantics.
2. **Dedicated outbox schema:** OutboxDbContext uses 'outbox' schema to keep MassTransit tables isolated from domain tables.
3. **Schema-specific migration history:** Each DbContext stores migrations in its own schema's __EFMigrationsHistory table for clean separation.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Domain event infrastructure complete with transactional outbox
- Events from aggregates will be captured and published reliably to Azure Service Bus
- Old in-process dispatch deprecated but still available for backward compatibility
- Ready for Plan 01-05 (Template module & CQRS guidelines)

---
*Phase: 01-foundation-project-structure*
*Completed: 2026-01-29*
