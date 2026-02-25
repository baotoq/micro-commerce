# Roadmap: MicroCommerce

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-02-13) — [archive](milestones/v1.0-ROADMAP.md)
- ✅ **v1.1 User Features** — Phases 11-14.3 (shipped 2026-02-14) — [archive](milestones/v1.1-ROADMAP.md)
- 🚧 **v2.0 DDD Foundation** — Phases 15-22 (in progress)

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
**Plans**: 2 plans

Plans:
- [x] 15-01-PLAN.md — Entity hierarchy + interfaces in BuildingBlocks (Entity<TId>, BaseAggregateRoot refactor, IAuditable, IConcurrencyToken, ISoftDeletable, AuditableAggregateRoot)
- [x] 15-02-PLAN.md — Interceptors (Audit, Concurrency, SoftDelete) + convention helpers + registration + DbUpdateConcurrencyException handling

#### Phase 16: Conventions - DRY Configuration
**Goal**: Eliminate repetitive manual EF Core configuration through model conventions
**Depends on**: Phase 15 (requires interfaces and base classes)
**Requirements**: MOD-01, MOD-02, MOD-03
**Success Criteria** (what must be TRUE):
  1. StronglyTypedId types are automatically converted in EF Core without manual HasConversion calls
  2. IConcurrencyToken entities automatically get IsConcurrencyToken configuration without manual [Timestamp] attributes
  3. Obsolete ValueObject base class removed from codebase with no remaining references
  4. Entity configuration files across 8 DbContexts are simpler with reduced boilerplate
**Plans**: 2 plans

Plans:
- [ ] 16-01-PLAN.md — Conventions (StronglyTypedId, IAuditable, IConcurrencyToken, ISoftDeletable) + BaseDbContext + snake_case naming + migrate all 8 DbContexts
- [ ] 16-02-PLAN.md — Configuration cleanup (remove redundant HasConversion/IsRowVersion/ToTable) + ValueObject removal + EF Core migrations

### Phase 16.1: Adopt Vogen for value object (INSERTED)

**Goal:** Replace hand-rolled StronglyTypedId<T> base class and 14 ID types with Vogen source-generated record structs, gaining validation, EF Core converters, and JSON serialization with zero runtime overhead
**Depends on:** Phase 16
**Requirements**: PRIM-01
**Plans:** 2/2 plans complete

Plans:
- [ ] 16.1-01-PLAN.md — Install Vogen, rewrite 14 ID types, update EF Core infrastructure, fix all call sites
- [ ] 16.1-02-PLAN.md — Generate EF Core migrations for all 8 DbContexts

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
**Plans**: 2 plans

Plans:
- [ ] 17-01-PLAN.md — FluentResults infrastructure: package install, ResultExtensions, ResultValidationBehavior, MediatR registration, ADR-006
- [ ] 17-02-PLAN.md — Pilot adoption: migrate UpdateOrderStatus and AdjustStock handlers to return Result, update endpoints

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
**Plans**: 2 plans

Plans:
- [ ] 18-01-PLAN.md — SmartEnum infrastructure: install Ardalis.SmartEnum packages, create generic EF Core string converter, replace OrderStatus and ProductStatus enums with SmartEnum types with transition rules, register EF Core + JSON converters
- [ ] 18-02-PLAN.md — SmartEnum migration: migrate Order/Product entity methods to TransitionTo(), update query handlers to TryFromName(), remove HasConversion<string>() from configs, update ChangeProductStatus handler for all transitions

#### Phase 19: Specification Pattern - Complex Query Logic
**Goal**: Extract complex query logic into reusable, testable Specification objects
**Depends on**: Phase 16 (conventions stable)
**Requirements**: QUERY-01, QUERY-02, QUERY-03
**Success Criteria** (what must be TRUE):
  1. Specification pattern base classes (ISpecification, Specification) integrated with EF Core via SpecificationEvaluator
  2. Complex catalog queries (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec) extracted from handlers into specifications
  3. Ordering queries (ActiveOrdersSpec, OrdersByBuyerSpec) use specifications for filtering
  4. Specifications are unit-testable in isolation from EF Core and database
  5. Query handlers demonstrate specification composition for complex filters (multiple Query.Where calls or chained WithSpecification)
**Plans**: 2 plans

Plans:
- [ ] 19-01-PLAN.md — Install Ardalis.Specification packages, create 5 catalog specifications (ProductsBase, PublishedProducts, ProductsByCategory, ProductSearch, ProductByStatus), refactor GetProductsQueryHandler to spec composition
- [ ] 19-02-PLAN.md — Create 2 ordering specifications (ActiveOrdersSpec excluding terminal statuses, OrdersByBuyerSpec), refactor GetAllOrders and GetOrdersByBuyer handlers to use specs

#### Phase 20: Integration Testing Infrastructure
**Goal**: Set up integration testing foundation with xUnit, WebApplicationFactory, and Testcontainers, proving the pattern with one test per feature
**Depends on**: Phase 19 (all building blocks proven, ready for validation)
**Requirements**: TEST-01
**Success Criteria** (what must be TRUE):
  1. MicroCommerceWebAppFactory configured with PostgreSQL Testcontainer and fake auth handler
  2. IntegrationTestBase provides helpers: authenticated/guest HttpClient creation, feature-scoped DbContext access, test data builders
  3. One representative API endpoint test per feature (7 features: Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists)
  4. One representative handler-level test demonstrating edge case/business rule testing pattern
  5. All integration tests pass with per-test-class database isolation using EnsureCreated
**Plans**: 2 plans

Plans:
- [x] 20-01-PLAN.md — Fix ApiWebApplicationFactory (MassTransit health check bug, EnsureCreated, 3 missing DbContexts), add FakeAuthenticationHandler, create IntegrationTestBase with auth/guest client helpers
- [ ] 20-02-PLAN.md — Add 3 missing feature endpoint tests (Profiles, Reviews, Wishlists), handler-level UpdateOrderStatus test, test data builders (ProductBuilder, OrderBuilder)

#### Phase 21: Adoption - Full Building Block Integration
**Goal**: Apply all new building blocks across all 7 feature modules with comprehensive test coverage
**Depends on**: Phases 15-20 (all building blocks proven)
**Requirements**: ADOPT-01, ADOPT-02, ADOPT-03, ADOPT-04, ADOPT-05, ADOPT-06, ADOPT-07, MOD-04
**Success Criteria** (what must be TRUE):
  1. All child entities across 7 modules (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists) inherit from Entity base
  2. All aggregates needing timestamps use AuditableAggregateRoot or implement IAuditable explicitly
  3. All existing optimistic concurrency (Order, Cart, StockItem) migrated from xmin to IConcurrencyToken with explicit Version column
  4. All 15+ StronglyTypedId types migrated to source generator with manual configurations removed
  5. Additional command handlers beyond pilot (2+ more) adopt Result pattern for business rule validation
  6. Complex catalog and ordering queries use Specification pattern instead of inline LINQ
  7. All 180+ existing tests pass with no regressions after migration
**Plans**: 3 plans

Plans:
- [ ] 21-01-PLAN.md — Entity<TId> base class migration for child entities + AuditableAggregateRoot migration for aggregates
- [ ] 21-02-PLAN.md — IConcurrencyToken migration (xmin to int Version) + EF Core migrations for 6 DbContexts
- [ ] 21-03-PLAN.md — Result pattern expansion (ChangeProductStatus, UpdateCartItem) + OpenAPI schema transformers + final verification

#### Phase 22: Wire Interceptors to DbContexts (Gap Closure)
**Goal**: Wire AuditInterceptor, ConcurrencyInterceptor, and SoftDeleteInterceptor to all 8 DbContexts via AddInterceptors() so cross-cutting behaviors fire at runtime
**Depends on**: Phase 21 (all building blocks adopted; interceptors exist but are not wired)
**Requirements**: ENTITY-02, ENTITY-04, ENTITY-05, ADOPT-03
**Gap Closure**: Closes all gaps from v2.0-MILESTONE-AUDIT.md
**Success Criteria** (what must be TRUE):
  1. All 3 interceptors (Audit, Concurrency, SoftDelete) passed to AddInterceptors() in all 8 DbContext registrations in Program.cs
  2. AuditInterceptor auto-sets CreatedAt on insert and UpdatedAt on update for IAuditable entities
  3. ConcurrencyInterceptor initializes Version=1 on insert and increments on update for IConcurrencyToken entities
  4. SoftDeleteInterceptor converts hard deletes to soft deletes for ISoftDeletable entities (infrastructure verified even if no entity uses it yet)
  5. Integration test verifies interceptor behavior: timestamps set, Version increments, concurrent update rejected with 409
  6. All existing 177 tests pass with no regressions
**Plans**: 1 plan

Plans:
- [ ] 22-01-PLAN.md — Add AddInterceptors() to all 8 DbContext registrations + integration tests for interceptor behavior

## Progress

**Execution Order:**
Phases execute in numeric order: 15 → 16 → 17 → 18 → 19 → 20 → 21 → 22

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1-10 | v1.0 MVP | 49/49 | Complete | 2026-02-13 |
| 11-14.3 | v1.1 User Features | 23/23 | Complete | 2026-02-14 |
| 15. Foundation | v2.0 DDD Foundation | 2/2 | Complete | 2026-02-14 |
| 16. Conventions | 2/2 | Complete    | 2026-02-24 | - |
| 17. Result Pattern | 2/2 | Complete    | 2026-02-24 | - |
| 18. Enumeration | 2/2 | Complete    | 2026-02-24 | - |
| 19. Specification | 2/2 | Complete    | 2026-02-24 | - |
| 20. Integration Testing | 2/2 | Complete    | 2026-02-25 | - |
| 21. Adoption | 3/3 | Complete    | 2026-02-25 | - |
| 22. Wire Interceptors | 1/1 | Complete    | 2026-02-25 | - |

---
*Roadmap created: 2026-01-29*
*v1.0 shipped: 2026-02-13*
*v1.1 shipped: 2026-02-14*
*v2.0 added: 2026-02-14*
