# Requirements: MicroCommerce

**Defined:** 2026-02-14
**Core Value:** A user can complete a purchase end-to-end

## v2.0 Requirements

Requirements for DDD Foundation milestone. Each maps to roadmap phases.

### Entity Infrastructure

- [ ] **ENTITY-01**: Entity base class with typed ID for child entities (CartItem, OrderItem, StockReservation, StockAdjustment)
- [ ] **ENTITY-02**: IAuditable interface with CreatedAt/UpdatedAt, auto-set via AuditInterceptor on SaveChanges
- [ ] **ENTITY-03**: AuditableAggregateRoot extending BaseAggregateRoot with IAuditable for aggregates needing timestamps
- [ ] **ENTITY-04**: IConcurrencyToken interface with explicit Version column, replacing xmin where used
- [ ] **ENTITY-05**: ISoftDeletable interface with IsDeleted/DeletedAt, global query filter via EF Core, SoftDeleteInterceptor

### Domain Primitives

- [ ] **PRIM-01**: StronglyTypedId source generator (Meziantou) with auto JSON, EF Core, and TypeConverter converters for all 15+ ID types
- [ ] **PRIM-02**: Enumeration/SmartEnum base (Ardalis.SmartEnum) with EF Core value converter and custom JsonConverter
- [ ] **PRIM-03**: Migrate OrderStatus to SmartEnum with state transition behavior (CanTransitionTo)
- [ ] **PRIM-04**: Migrate ProductStatus to SmartEnum with publish/archive behavior
- [ ] **PRIM-05**: Result type (FluentResults) integrated into BuildingBlocks with Result extensions for HTTP responses
- [ ] **PRIM-06**: ResultValidationBehavior for MediatR pipeline coexisting with existing ValidationBehavior

### Query Patterns

- [ ] **QUERY-01**: Specification pattern base classes (Ardalis.Specification) integrated with EF Core DbContext
- [ ] **QUERY-02**: Catalog specifications (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec)
- [ ] **QUERY-03**: Ordering specifications (ActiveOrdersSpec, OrdersByBuyerSpec)

### Modernization

- [ ] **MOD-01**: EF Core conventions for StronglyTypedId auto value converters (eliminate manual HasConversion)
- [ ] **MOD-02**: EF Core conventions for concurrency token auto-configuration on IConcurrencyToken entities
- [ ] **MOD-03**: Remove obsolete ValueObject base class and any dead infrastructure code
- [ ] **MOD-04**: OpenAPI schema filters for StronglyTypedId (primitive display) and Enumeration (string display)

### Adoption

- [ ] **ADOPT-01**: Migrate all child entities across 7 modules to Entity base class
- [ ] **ADOPT-02**: Migrate all aggregates to AuditableAggregateRoot or apply IAuditable where appropriate
- [ ] **ADOPT-03**: Migrate existing optimistic concurrency (Order, Cart, StockItem) from xmin to IConcurrencyToken with explicit Version
- [ ] **ADOPT-04**: Migrate all StronglyTypedId types to use source generator, remove manual converter configurations
- [ ] **ADOPT-05**: Adopt Result pattern in 2+ command handlers as pilot (e.g., SubmitOrder, ProcessPayment)
- [ ] **ADOPT-06**: Apply Specification pattern to complex catalog and ordering queries
- [ ] **ADOPT-07**: All existing tests pass after migration with no regressions

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
| ENTITY-01 | — | Pending |
| ENTITY-02 | — | Pending |
| ENTITY-03 | — | Pending |
| ENTITY-04 | — | Pending |
| ENTITY-05 | — | Pending |
| PRIM-01 | — | Pending |
| PRIM-02 | — | Pending |
| PRIM-03 | — | Pending |
| PRIM-04 | — | Pending |
| PRIM-05 | — | Pending |
| PRIM-06 | — | Pending |
| QUERY-01 | — | Pending |
| QUERY-02 | — | Pending |
| QUERY-03 | — | Pending |
| MOD-01 | — | Pending |
| MOD-02 | — | Pending |
| MOD-03 | — | Pending |
| MOD-04 | — | Pending |
| ADOPT-01 | — | Pending |
| ADOPT-02 | — | Pending |
| ADOPT-03 | — | Pending |
| ADOPT-04 | — | Pending |
| ADOPT-05 | — | Pending |
| ADOPT-06 | — | Pending |
| ADOPT-07 | — | Pending |

**Coverage:**
- v2.0 requirements: 25 total
- Mapped to phases: 0
- Unmapped: 25 ⚠️

---
*Requirements defined: 2026-02-14*
*Last updated: 2026-02-14 after initial definition*
