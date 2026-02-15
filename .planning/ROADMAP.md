# Roadmap: MicroCommerce

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-02-13) — [archive](milestones/v1.0-ROADMAP.md)
- ✅ **v1.1 User Features** — Phases 11-14.3 (shipped 2026-02-14) — [archive](milestones/v1.1-ROADMAP.md)
- 🚧 **v2.0 DDD Foundation** — Phases 15-21 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-10) — SHIPPED 2026-02-13</summary>

- [x] Phase 1: Foundation & Project Structure (6/6 plans) — completed 2026-01-30
- [x] Phase 2: Catalog Domain & Admin CRUD (7/7 plans) — completed 2026-01-30
- [x] Phase 3: Catalog Storefront & Seed Data (6/6 plans) — completed 2026-02-07
- [x] Phase 4: Inventory Domain (5/5 plans) — completed 2026-02-08
- [x] Phase 5: Event Bus Infrastructure (3/3 plans) — completed 2026-02-09
- [x] Phase 6: Cart Domain (4/4 plans) — completed 2026-02-09
- [x] Phase 7: Ordering Domain & Checkout (4/4 plans) — completed 2026-02-10
- [x] Phase 8: Order History & Management (5/5 plans) — completed 2026-02-12
- [x] Phase 9: API Gateway (3/3 plans) — completed 2026-02-12
- [x] Phase 10: Testing & Polish (6/6 plans) — completed 2026-02-13

</details>

<details>
<summary>✅ v1.1 User Features (Phases 11-14.3) — SHIPPED 2026-02-14</summary>

- [x] Phase 11: User Profiles & Authentication Flow (5/5 plans) — completed 2026-02-13
- [x] Phase 12: Product Reviews & Ratings (3/3 plans) — completed 2026-02-13
- [x] Phase 13: Wishlists & Saved Items (3/3 plans) — completed 2026-02-13
- [x] Phase 14: Integration & Polish (3/3 plans) — completed 2026-02-14
- [x] Phase 14.1: Check DDD Approach Correctness (2/2 plans) — completed 2026-02-14
- [x] Phase 14.2: ValueObject Record Struct Migration (3/3 plans) — completed 2026-02-14
- [x] Phase 14.3: DDD Audit Issue Fixes (4/4 plans) — completed 2026-02-14

</details>

### 🚧 v2.0 DDD Foundation (In Progress)

**Milestone Goal:** Strengthen and modernize DDD building blocks with full adoption across all features

#### Phase 15: Foundation - Entity Base & Audit Infrastructure
**Goal**: Standardize entity hierarchy and automatic audit field management across all modules
**Depends on**: Nothing (first phase of milestone)
**Requirements**: ENTITY-01, ENTITY-02, ENTITY-03, ENTITY-04, ENTITY-05
**Success Criteria** (what must be TRUE):
  1. Child entities (CartItem, OrderItem, StockReservation, StockAdjustment) inherit from Entity base with typed ID
  2. Aggregates using IAuditable have CreatedAt/UpdatedAt automatically set on SaveChanges without manual code
  3. AuditableAggregateRoot provides combined base class for aggregates needing both domain events and timestamps
  4. Entities implementing IConcurrencyToken have explicit Version column configured by conventions
  5. Soft-deletable entities using ISoftDeletable are automatically filtered from queries and marked deleted via interceptor
**Plans**: TBD

Plans:
- [ ] 15-01: TBD
- [ ] 15-02: TBD

#### Phase 16: Conventions - DRY Configuration
**Goal**: Eliminate repetitive manual EF Core configuration through model conventions
**Depends on**: Phase 15 (requires interfaces and base classes)
**Requirements**: MOD-01, MOD-02, MOD-03
**Success Criteria** (what must be TRUE):
  1. StronglyTypedId types are automatically converted in EF Core without manual HasConversion calls
  2. IConcurrencyToken entities automatically get IsConcurrencyToken configuration without manual [Timestamp] attributes
  3. Obsolete ValueObject base class removed from codebase with no remaining references
  4. Entity configuration files across 8 DbContexts are simpler with reduced boilerplate
**Plans**: TBD

Plans:
- [ ] 16-01: TBD
- [ ] 16-02: TBD

#### Phase 17: Result Pattern - Explicit Error Handling
**Goal**: Introduce Result type for railway-oriented programming with pilot adoption in command handlers
**Depends on**: Phase 15 (foundation stable)
**Requirements**: PRIM-05, PRIM-06
**Success Criteria** (what must be TRUE):
  1. FluentResults Result/Result types available in BuildingBlocks.Common with extension methods for HTTP response mapping
  2. ResultValidationBehavior coexists with existing ValidationBehavior in MediatR pipeline based on TResponse constraint
  3. Two command handlers (pilot) return Result instead of throwing exceptions for business rule violations
  4. ADR documents Result/Exception boundary: business rules return Result, invalid input throws exception
  5. Pilot handlers demonstrate clear error aggregation with multiple Result.Failure reasons
**Plans**: TBD

Plans:
- [ ] 17-01: TBD
- [ ] 17-02: TBD

#### Phase 18: Enumeration - Enums with Behavior
**Goal**: Replace plain enums with SmartEnum for type-safe enumerations with encapsulated behavior
**Depends on**: Phase 17 (Result pattern available for error handling)
**Requirements**: PRIM-02, PRIM-03, PRIM-04
**Success Criteria** (what must be TRUE):
  1. Enumeration base class integrated with EF Core value converter and JSON serialization (string format, not object)
  2. OrderStatus migrated to SmartEnum with CanTransitionTo() validation preventing invalid state changes
  3. ProductStatus migrated to SmartEnum with publish/archive behavior encapsulated in type
  4. Frontend API contracts unchanged (still receive "Pending" string, not object)
  5. All existing order status transitions validated at compile-time through SmartEnum methods
**Plans**: TBD

Plans:
- [ ] 18-01: TBD
- [ ] 18-02: TBD

#### Phase 19: Specification Pattern - Complex Query Logic
**Goal**: Extract complex query logic into reusable, testable Specification objects
**Depends on**: Phase 16 (conventions stable)
**Requirements**: QUERY-01, QUERY-02, QUERY-03
**Success Criteria** (what must be TRUE):
  1. Specification pattern base classes (ISpecification, Specification) integrated with EF Core via SpecificationEvaluator
  2. Complex catalog queries (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec) extracted from handlers into specifications
  3. Ordering queries (ActiveOrdersSpec, OrdersByBuyerSpec) use specifications for filtering and pagination
  4. Specifications are unit-testable in isolation from EF Core and database
  5. Query handlers demonstrate specification composition via And/Or for complex filters
**Plans**: TBD

Plans:
- [ ] 19-01: TBD
- [ ] 19-02: TBD

#### Phase 20: Source Generators - StronglyTypedId Improvements
**Goal**: Automate StronglyTypedId converter generation through source generators
**Depends on**: Phase 16 (existing manual pattern validated)
**Requirements**: PRIM-01, MOD-04
**Success Criteria** (what must be TRUE):
  1. Meziantou.Framework.StronglyTypedId source generator integrated with auto-generated JSON, EF Core, and TypeConverter converters
  2. New StronglyTypedId types (pilot with 2-3 IDs) use generator instead of manual record declarations
  3. OpenAPI schema filters display StronglyTypedId as primitives (Guid/int) instead of nested objects in Swagger UI
  4. Migration path documented for gradual adoption of remaining 15+ ID types
  5. Build performance acceptable with source generator enabled (no significant compilation slowdown)
**Plans**: TBD

Plans:
- [ ] 20-01: TBD
- [ ] 20-02: TBD

#### Phase 21: Adoption - Full Building Block Integration
**Goal**: Apply all new building blocks across all 7 feature modules with comprehensive test coverage
**Depends on**: Phases 15-20 (all building blocks proven)
**Requirements**: ADOPT-01, ADOPT-02, ADOPT-03, ADOPT-04, ADOPT-05, ADOPT-06, ADOPT-07
**Success Criteria** (what must be TRUE):
  1. All child entities across 7 modules (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists) inherit from Entity base
  2. All aggregates needing timestamps use AuditableAggregateRoot or implement IAuditable explicitly
  3. All existing optimistic concurrency (Order, Cart, StockItem) migrated from xmin to IConcurrencyToken with explicit Version column
  4. All 15+ StronglyTypedId types migrated to source generator with manual configurations removed
  5. Additional command handlers beyond pilot (2+ more) adopt Result pattern for business rule validation
  6. Complex catalog and ordering queries use Specification pattern instead of inline LINQ
  7. All 180+ existing tests pass with no regressions after migration
**Plans**: TBD

Plans:
- [ ] 21-01: TBD
- [ ] 21-02: TBD
- [ ] 21-03: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 15 → 16 → 17 → 18 → 19 → 20 → 21

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1-10 | v1.0 MVP | 49/49 | Complete | 2026-02-13 |
| 11-14.3 | v1.1 User Features | 23/23 | Complete | 2026-02-14 |
| 15. Foundation | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 16. Conventions | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 17. Result Pattern | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 18. Enumeration | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 19. Specification | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 20. Source Generators | v2.0 DDD Foundation | 0/TBD | Not started | - |
| 21. Adoption | v2.0 DDD Foundation | 0/TBD | Not started | - |

---
*Roadmap created: 2026-01-29*
*v1.0 shipped: 2026-02-13*
*v1.1 shipped: 2026-02-14*
*v2.0 added: 2026-02-14*
