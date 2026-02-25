# Requirements: MicroCommerce

**Defined:** 2026-02-14
**Core Value:** A user can complete a purchase end-to-end

## v2.0 Requirements

Requirements for DDD Foundation milestone. Each maps to roadmap phases.

### Entity Infrastructure

- [x] **ENTITY-01**: Entity base class with typed ID for child entities (CartItem, OrderItem, StockReservation, StockAdjustment)
- [x] **ENTITY-02**: IAuditable interface with CreatedAt/UpdatedAt, auto-set via AuditInterceptor on SaveChanges
- [x] **ENTITY-03**: AuditableAggregateRoot extending BaseAggregateRoot with IAuditable for aggregates needing timestamps
- [x] **ENTITY-04**: IConcurrencyToken interface with explicit Version column, replacing xmin where used
- [x] **ENTITY-05**: ISoftDeletable interface with IsDeleted/DeletedAt, global query filter via EF Core, SoftDeleteInterceptor

### Domain Primitives

- [x] **PRIM-01**: StronglyTypedId source generator (Meziantou) with auto JSON, EF Core, and TypeConverter converters for all 15+ ID types
- [x] **PRIM-02**: Enumeration/SmartEnum base (Ardalis.SmartEnum) with EF Core value converter and custom JsonConverter
- [x] **PRIM-03**: Migrate OrderStatus to SmartEnum with state transition behavior (CanTransitionTo)
- [x] **PRIM-04**: Migrate ProductStatus to SmartEnum with publish/archive behavior
- [x] **PRIM-05**: Result type (FluentResults) integrated into BuildingBlocks with Result extensions for HTTP responses
- [x] **PRIM-06**: ResultValidationBehavior for MediatR pipeline coexisting with existing ValidationBehavior

### Query Patterns

- [x] **QUERY-01**: Specification pattern base classes (Ardalis.Specification) integrated with EF Core DbContext
- [x] **QUERY-02**: Catalog specifications (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec)
- [x] **QUERY-03**: Ordering specifications (ActiveOrdersSpec, OrdersByBuyerSpec)

### Modernization

- [x] **MOD-01**: EF Core conventions for StronglyTypedId auto value converters (eliminate manual HasConversion)
- [x] **MOD-02**: EF Core conventions for concurrency token auto-configuration on IConcurrencyToken entities
- [x] **MOD-03**: Remove obsolete ValueObject base class and any dead infrastructure code
- [x] **MOD-04**: OpenAPI schema filters for StronglyTypedId (primitive display) and Enumeration (string display)

### Testing

- [x] **TEST-01**: Integration test infrastructure with WebApplicationFactory + Testcontainers, one representative test per feature (7 features)

### Adoption

- [x] **ADOPT-01**: Migrate all child entities across 7 modules to Entity base class
- [x] **ADOPT-02**: Migrate all aggregates to AuditableAggregateRoot or apply IAuditable where appropriate
- [x] **ADOPT-03**: Migrate existing optimistic concurrency (Order, Cart, StockItem) from xmin to IConcurrencyToken with explicit Version
- [x] **ADOPT-04**: Migrate all StronglyTypedId types to use source generator, remove manual converter configurations
- [x] **ADOPT-05**: Adopt Result pattern in 2+ command handlers as pilot (e.g., SubmitOrder, ProcessPayment)
- [x] **ADOPT-06**: Apply Specification pattern to complex catalog and ordering queries
- [x] **ADOPT-07**: All existing tests pass after migration with no regressions

## Future Requirements

### Extended Adoption

- **ADOPT-F01**: Adopt Result pattern across all command handlers (beyond pilot)
- **ADOPT-F02**: Add specifications for Reviews, Wishlists, Profiles queries
- **ADOPT-F03**: CreatedBy/ModifiedBy user tracking via IHttpContextAccessor in AuditInterceptor

### Advanced Building Blocks

- **ADV-01**: Domain service base class for cross-aggregate operations
- **ADV-02**: Outbox pattern abstraction in BuildingBlocks (currently MassTransit handles this)
- **ADV-03**: Multi-tenancy interface (ITenantEntity) for future SaaS expansion

## Out of Scope

| Feature | Reason |
|---------|--------|
| Generic Repository | EF Core DbContext already is unit-of-work + repository, avoid leaky abstraction |
| Auto-mapper for aggregates | Domain objects should map explicitly, magic mapping hides logic |
| Custom ORM wrapper | EF Core is already the abstraction layer |
| Transaction script helpers | Undermines DDD aggregate pattern |
| Event sourcing primitives | Adds complexity without proportional demo value (already out of scope) |
| CreatedBy/ModifiedBy user tracking | Requires IHttpContextAccessor integration, defer to future |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| ENTITY-01 | Phase 15 | Complete |
| ENTITY-02 | Phase 22 | Complete |
| ENTITY-03 | Phase 15 | Complete |
| ENTITY-04 | Phase 22 | Complete |
| ENTITY-05 | Phase 22 | Complete |
| PRIM-01 | Phase 16.1 | Complete |
| PRIM-02 | Phase 18 | Complete |
| PRIM-03 | Phase 18 | Complete |
| PRIM-04 | Phase 18 | Complete |
| PRIM-05 | Phase 17 | Complete |
| PRIM-06 | Phase 17 | Complete |
| QUERY-01 | Phase 19 | Complete |
| QUERY-02 | Phase 19 | Complete |
| QUERY-03 | Phase 19 | Complete |
| MOD-01 | Phase 16 | Complete |
| MOD-02 | Phase 16 | Complete |
| MOD-03 | Phase 16 | Complete |
| MOD-04 | Phase 21 | Complete |
| TEST-01 | Phase 20 | Complete |
| ADOPT-01 | Phase 21 | Complete |
| ADOPT-02 | Phase 21 | Complete |
| ADOPT-03 | Phase 22 | Complete |
| ADOPT-04 | Phase 21 | Complete |
| ADOPT-05 | Phase 21 | Complete |
| ADOPT-06 | Phase 21 | Complete |
| ADOPT-07 | Phase 21 | Complete |

**Coverage:**
- v2.0 requirements: 26 total
- Mapped to phases: 26 (100%)
- Unmapped: 0 ✓

---
*Requirements defined: 2026-02-14*
*Last updated: 2026-02-25 after v2.0 milestone audit gap closure*
