# Phase 1: Foundation & Project Structure - Context

**Gathered:** 2026-01-29
**Status:** Ready for planning

<domain>
## Phase Boundary

Establish modular monolith structure with clear bounded contexts, shared building blocks, and development patterns. This phase sets up the architectural foundation that all other phases build upon — module organization, DbContexts per module, MediatR pipeline with validation, and domain event infrastructure.

</domain>

<decisions>
## Implementation Decisions

### Module Organization
- Folders first, projects later — start with Catalog/, Cart/, Ordering/, Inventory/ as top-level folders within ApiService
- Relaxed boundaries for now — direct references allowed within monolith, will enforce stricter boundaries when extracting services
- Clean Architecture layers within each module — Domain/, Application/, Infrastructure/ structure
- Use existing BuildingBlocks for shared primitives — but audit against best practices before using (aggregates, value objects, domain events)

### Project Structure Conventions
- Explicit suffixes for class naming — ProductService, IProductRepository, CreateProductCommand, ProductCreatedDomainEvent
- Pragmatic/hybrid approach — not strictly following eShop or Ardalis, whatever makes sense for this project

### CQRS Guidelines
- Full CQRS everywhere — every operation uses Command/Query + Handler pattern
- Strict handler separation — one handler per command/query, no shared services between handlers
- Same database for reads and writes — no separate read models, use same Postgres via EF Core
- Pipeline validation — FluentValidation in MediatR pipeline behavior, not inside handlers

### Domain Event Patterns
- Service Bus from start — use Azure Service Bus from Phase 1, not MediatR in-process (roadmap adjustment)
- Past tense naming with suffix — ProductCreatedDomainEvent, OrderPlacedDomainEvent
- Thin events — events carry IDs only, consumers fetch details when needed
- Transactional outbox — events saved to outbox table with aggregate, background worker publishes to Service Bus

### Claude's Discretion
- File organization for domain types (one per file vs grouped by aggregate)
- EF Core migrations organization (per-module vs shared folder)
- Specific outbox implementation approach (MassTransit built-in vs custom)

</decisions>

<specifics>
## Specific Ideas

- "Audit existing BuildingBlocks against best practices before using" — ensure the existing DDD primitives follow current patterns
- Service Bus from day one changes the Phase 5 scope — event bus infrastructure moves earlier

</specifics>

<deferred>
## Deferred Ideas

- Service extraction to separate projects — happens when extracting to true microservices (post-v1)
- Strict module boundaries enforcement — revisit when preparing for service extraction

</deferred>

---

*Phase: 01-foundation-project-structure*
*Context gathered: 2026-01-29*
