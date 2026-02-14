# Project Research Summary

**Project:** MicroCommerce v2.0 - DDD Foundation Milestone
**Domain:** DDD Building Blocks for .NET 10 Modular Monolith E-Commerce Platform
**Researched:** 2026-02-14
**Confidence:** HIGH

## Executive Summary

MicroCommerce is a .NET 10 modular monolith e-commerce platform demonstrating modern microservices-ready architecture. The v2.0 milestone focuses on strengthening the DDD foundation by adding enterprise-grade building blocks: Entity base classes with audit fields, optimistic concurrency, Result type for explicit error handling, Enumeration classes with behavior, Specification pattern for complex queries, and source generators for StronglyTypedId improvements. These additions integrate with the existing BaseAggregateRoot, DomainEvent infrastructure, and CQRS/MediatR pipeline.

The recommended approach is **additive, not disruptive**: new building blocks are opt-in, allowing gradual migration without breaking existing code. Core strategy uses battle-tested libraries (FluentResults for Result pattern, Ardalis.SmartEnum for enumerations, Ardalis.Specification for queries, Meziantou.Framework.StronglyTypedId for source generation) to avoid reinventing foundational patterns. Integration happens at three key points: EF Core SaveChangesInterceptors for cross-cutting concerns like audit timestamps, MediatR pipeline behaviors for Result-based validation, and EF Core model conventions for automatic value converter application.

Key risks center on backward compatibility and integration conflicts: (1) Entity base classes triggering unwanted EF Core TPH inheritance mappings, (2) audit interceptors conflicting with existing domain-driven timestamp management, (3) PostgreSQL xmin concurrency token limitations during backup/restore, (4) Result pattern creating inconsistent error handling when mixed with existing exception-based flow. Prevention strategy: explicit EF Core configuration testing in Phase 1, timestamp ownership audit before interceptor adoption in Phase 2, explicit versioning evaluation in Phase 3, and architectural decision record for Result/Exception boundaries in Phase 4.

## Key Findings

### Recommended Stack

The stack builds on existing .NET 10, EF Core 10, MediatR, and PostgreSQL foundations with four targeted library additions and one build-time source generator. All choices prioritize .NET ecosystem standards, zero runtime dependencies, and seamless EF Core integration.

**Core technologies:**
- **FluentResults 4.0.0**: Result pattern implementation — mature library with .NET 10 compatibility via .NET Standard 2.0, supports multiple errors per result with rich metadata, zero dependencies, better for complex error scenarios than exception-driven flow
- **Ardalis.SmartEnum 8.2.0**: Strongly-typed enumeration replacement — de facto standard in .NET DDD community, supports rich behavior and custom properties, integrates with EF Core 7+ via value converters
- **Ardalis.Specification 9.3.1**: Query specification pattern — standard for DDD specifications in .NET, integrates seamlessly with EF Core, promotes query reuse and testability, used in Microsoft eShopOnWeb reference architecture
- **Meziantou.Framework.StronglyTypedId 2.3.11**: Source generator for StronglyTypedId — most comprehensive generator for the pattern, zero runtime dependencies, generates all needed converters (System.Text.Json, TypeConverter, EF Core), actively maintained (updated Jan 2026)

**Supporting EF Core integration:**
- Ardalis.Specification.EntityFrameworkCore 9.3.1 for specification evaluation
- Ardalis.SmartEnum.EFCore 8.2.0 for automatic value conversion
- All packages forward-compatible with EF Core 10.0.3

**What to avoid:**
- andrewlock/StronglyTypedId (still in beta 1.0.0-beta08 after 2+ years)
- Custom ValueObject base class (already obsoleted, readonly record struct is superior)
- Audit.EntityFramework.Core (too heavyweight for simple timestamp tracking)
- Custom Result implementation (FluentResults is battle-tested)

### Expected Features

Research identified six table stakes features for comprehensive DDD building blocks, five differentiating features that provide significant value for domain-rich applications, and three anti-features to explicitly avoid.

**Must have (table stakes):**
- **Entity Base Class** — standardizes ID handling, equality, and child entity patterns across all 7 feature modules (currently only aggregates have BaseAggregateRoot, child entities like CartItem/OrderItem lack standard base)
- **Audit Field Interfaces** — automatic CreatedAt/ModifiedAt tracking via SaveChangesInterceptor (currently manual per aggregate, industry standard is interface-based automation)
- **Optimistic Concurrency Base** — standardizes RowVersion/Version pattern for aggregates needing concurrency control (currently manual [Timestamp] on Order.Version, should be opt-in base class/interface)
- **StronglyTypedId Converters** — auto-generated EF Core, System.Text.Json, TypeConverter support for all 15+ ID types (currently manual inheritance from StronglyTypedId base and manual EF configuration)
- **Specification Pattern** — reusable, composable query logic for complex catalog/ordering queries (currently ad-hoc LINQ scattered across handlers)
- **Enumeration/SmartEnum** — enums with behavior, encapsulation, type safety, EF Core persistence (currently plain enums like OrderStatus/ProductStatus with no centralized transition logic)

**Should have (competitive):**
- **Result type monad** — railway-oriented error handling eliminates exception-driven flow for business logic failures, enables explicit Result.Success/Failure instead of throwing domain exceptions
- **Domain Event Dispatcher (in-process)** — automatic MediatR notification handlers before SaveChanges (already have out-of-process via DomainEventInterceptor + MassTransit, could add in-process handlers)
- **Soft Delete Interface** — ISoftDeletable + interceptor for logical deletion pattern (Product.Archive is manual, could be interface-driven with global query filter)

**Defer (v2+):**
- Tenant ID Interface (single-tenant application, multi-tenancy is premature)
- Outbox Pattern Support (MassTransit handles this, would duplicate existing infrastructure)
- Generic Repository (EF Core DbContext already is unit-of-work + repository, avoid leaky abstraction)

**Migration strategy:**
All features are backward compatible and opt-in. StronglyTypedId generators are additive, audit interfaces apply progressively, concurrency is opt-in, SmartEnum requires breaking migration (coordinate with frontend), Result pattern is opt-in per command, Specifications are additive.

### Architecture Approach

Integration is **additive, not disruptive**: new building blocks extend the existing foundation (BaseAggregateRoot, StronglyTypedId, DomainEvent, DomainEventInterceptor) without replacing it. Existing aggregates continue working unchanged while new building blocks provide opt-in capabilities through inheritance (AuditableAggregateRoot extends BaseAggregateRoot) and marker interfaces (IAuditable, IConcurrencyToken).

**Major components:**
1. **Entity Hierarchy** — BuildingBlocks.Common gains Entity base for child entities, AuditableAggregateRoot for aggregates needing timestamps, IConcurrencyToken interface for version-tracked aggregates. Existing BaseAggregateRoot unchanged, existing aggregates opt-in to new bases.
2. **EF Core Interceptors & Conventions** — AuditInterceptor sets CreatedAt/UpdatedAt on IAuditable entities during SavingChanges, runs before existing DomainEventInterceptor. Model conventions (StronglyTypedIdConvention, ConcurrencyTokenConvention) auto-apply value converters and row versioning, eliminating repetitive configuration.
3. **MediatR Pipeline Extensions** — ResultValidationBehavior for Result-based handlers coexists with existing exception-based ValidationBehavior. MediatR resolves correct behavior based on TResponse constraint (Result vs plain type).
4. **Query Abstraction** — Specification pattern via ISpecification, base Specification class, and SpecificationEvaluator for EF Core query building. Used directly with DbContext (no repository layer needed), testable in isolation, composable via And/Or.
5. **Domain Primitives** — FluentResults Result/Result types for explicit error handling, Enumeration base for SmartEnum pattern with business logic, source-generated StronglyTypedId with auto converters (JSON, EF Core, TypeConverter).
6. **JSON & OpenAPI Integration** — Custom JsonConverters for StronglyTypedId (serializes as primitive, not object) and Enumeration (serializes as string name), ISchemaFilter for correct OpenAPI schema generation (prevents nested object schemas).

**Data flow changes:**
Exception-based flow continues for infrastructure failures and invalid input (ValidationBehavior throws). Result-based flow is opt-in for business rule violations: ResultValidationBehavior returns Result.Failure instead of throwing. Audit timestamps set automatically via AuditInterceptor before SaveChanges, domain events published after SaveChanges by existing DomainEventInterceptor (unchanged). StronglyTypedId conversion handled transparently by conventions (no manual HasConversion calls).

**Integration points:**
- SaveChangesInterceptor stack: AuditInterceptor (SavingChanges) → DomainEventInterceptor (SavedChangesAsync)
- MediatR pipeline: ValidationBehavior (throws) OR ResultValidationBehavior (returns Result) based on TResponse type
- EF Core conventions: StronglyTypedIdConvention, ConcurrencyTokenConvention applied globally via ConfigureConventions

### Critical Pitfalls

Eight critical pitfalls identified, all with concrete prevention strategies and clear phase assignments for mitigation.

1. **Entity Base Class Breaking EF Core Mappings** — Adding Entity base triggers unwanted TPH (Table-Per-Hierarchy) mapping with discriminator columns. Prevention: explicitly mark Entity as unmapped via modelBuilder.Ignore(), test migration generation in Phase 1 Task 0.
2. **Audit Interceptor Double-Setting Timestamps** — Existing aggregates set CreatedAt in factory methods (Order.Create), interceptor overwrites causing conflicts. Prevention: choose ONE strategy (domain OR interceptor), audit existing entities before Phase 2, use marker interface only on entities that DON'T manage timestamps.
3. **PostgreSQL xmin Concurrency Token Loss on Backup/Restore** — xmin is transaction ID, not persistent version number, resets on backup/restore causing all updates to fail. Prevention: add explicit Version column alongside xmin in Phase 3, document xmin limitations, test backup/restore cycle.
4. **Result Pattern Inconsistency with Existing Exception Flow** — Mixing Result return types with exception-throwing handlers creates unpredictable error handling. Prevention: create ADR in Phase 4 Task 0 defining boundary (business rules → Result, invalid input → exception), never mix both in same handler.
5. **Enumeration Class Migration Breaking JSON Serialization** — Default Enumeration serialization produces object {"value": 1, "name": "Submitted"}, breaking API contracts expecting "Submitted". Prevention: implement custom JsonConverter in Phase 5 Task 0 before migrating any enums, test API response shape doesn't change.
6. **Specification IsSatisfiedBy N+1 Queries** — Using spec.IsSatisfiedBy(entity) in loops triggers lazy loads on navigation properties. Prevention: disable lazy loading globally, document Specifications work only on fully loaded entities or in query expressions, load test Phase 6 with 100+ entity collections.
7. **Source Generator Build Failures in Multi-Target Projects** — Source generators run per TFM, multi-targeting BuildingBlocks.Common causes duplicate type generation. Prevention: keep generator projects single-target .NET 10.0 in Phase 7 Task 0, test before production use.
8. **StronglyTypedId JSON Breaking OpenAPI Schema** — Custom JsonConverter works but OpenAPI schema shows nested object instead of primitive, breaking client generation. Prevention: implement ISchemaFilter alongside JsonConverter in Phase 8 Task 2, validate openapi.json schema output.

**Common technical debt patterns:**
- Skipping explicit EF Core inheritance configuration testing (NEVER acceptable, always test migration generation)
- Mixing Result and Exception without clear boundary (only during controlled migration with end date)
- Using IsSatisfiedBy in loops over database entities (NEVER, only for in-memory collections)
- Keeping xmin without explicit version column in production (local dev only)

## Implications for Roadmap

Based on research, suggested phase structure follows a **safe migration path** prioritizing low-risk foundations first, followed by behavior enrichment, then query patterns. Critical dependencies: AuditInterceptor requires IAuditable interface, ResultValidationBehavior requires Result type, Conventions require marker interfaces, Enumeration EF Core support requires EnumerationValueConverter.

### Phase 15: Foundation - Entity Base & Audit Infrastructure
**Rationale:** Lowest risk, highest impact. Entity base classes and audit interfaces are purely additive, no breaking changes. Establishes foundation for all subsequent phases.
**Delivers:** Entity base for child entities (OrderItem, CartItem), AuditableAggregateRoot for aggregates, IAuditable/IConcurrencyToken interfaces, AuditInterceptor with timestamp automation.
**Addresses:** Table stakes features (Entity Base Class, Audit Field Interfaces, Optimistic Concurrency Base from FEATURES.md).
**Avoids:** Pitfall 1 (EF Core TPH mapping), Pitfall 2 (timestamp conflicts).
**Research Flag:** Standard patterns, skip phase research. Include explicit migration testing in Task 0.

### Phase 16: EF Core Conventions - DRY Improvements
**Rationale:** Eliminates repetitive manual configuration after base classes exist. Non-breaking, pure refactoring, no behavior changes.
**Delivers:** StronglyTypedIdConvention for auto value converters, ConcurrencyTokenConvention for auto row versioning, removal of manual HasConversion calls from 30+ entity configurations.
**Uses:** Existing StronglyTypedId base class, IConcurrencyToken interface from Phase 15.
**Avoids:** Pitfall 3 (xmin backup/restore) by documenting limitations in Task 0.
**Research Flag:** Standard EF Core patterns, skip phase research.

### Phase 17: Result Pattern - Explicit Error Handling
**Rationale:** Foundational behavioral change, requires architectural decision before adoption. Pilot with 1-2 commands to validate approach before broader rollout.
**Delivers:** FluentResults integration, Result/Result types in BuildingBlocks.Common, ResultExtensions.ToHttpResult(), ResultValidationBehavior for MediatR pipeline.
**Addresses:** Differentiator feature (Result type monad from FEATURES.md).
**Avoids:** Pitfall 4 (Result/Exception mixing) via ADR creation in Task 0.
**Research Flag:** Needs phase research for ADR creation, error handling boundary definition, and pilot command selection.

### Phase 18: Enumeration Classes - Enums with Behavior
**Rationale:** Breaking migration requiring frontend coordination. Migrate selectively (only enums with behavior), defer simple label enums.
**Delivers:** Enumeration base class, EnumerationValueConverter for EF Core, EnumerationJsonConverter for API serialization, migration of OrderStatus and PaymentStatus.
**Uses:** Ardalis.SmartEnum library from STACK.md.
**Avoids:** Pitfall 5 (JSON serialization) by implementing JsonConverter in Task 0 before any enum migration.
**Research Flag:** Standard pattern (library-driven), skip phase research. Coordinate with frontend team for DTO changes.

### Phase 19: Specification Pattern - Complex Query Logic
**Rationale:** Highest complexity, defer until simpler patterns established. Adopt selectively for complex queries only.
**Delivers:** ISpecification/Specification interfaces, SpecificationEvaluator for EF Core, specifications for catalog filtering (PublishedProductsSpec, ProductsByCategorySpec) and order queries (ActiveOrdersSpec).
**Uses:** Ardalis.Specification patterns from STACK.md.
**Implements:** Query abstraction component from ARCHITECTURE.md.
**Avoids:** Pitfall 6 (N+1 queries) via lazy loading disabled globally, performance testing in final task.
**Research Flag:** Needs phase research for specification selection criteria, query candidates, and testability patterns.

### Phase 20: Source Generators - StronglyTypedId Improvements
**Rationale:** Optional optimization, defer until 20+ StronglyTypedId types exist. Build-time complexity warrants late adoption.
**Delivers:** Meziantou.Framework.StronglyTypedId integration, assembly-level converter defaults, StronglyTypedIdJsonConverterFactory, gradual migration of existing manual records.
**Uses:** Meziantou source generator from STACK.md.
**Avoids:** Pitfall 7 (multi-TFM build failures) by keeping generator project single-target, Pitfall 8 (OpenAPI schema) by adding ISchemaFilter.
**Research Flag:** Standard generator patterns, skip phase research. Pilot with new feature module before migrating existing IDs.

### Phase 21: Adoption - Migration Across 7 Feature Modules
**Rationale:** Apply all building blocks across Catalog, Cart, Ordering, Inventory, Messaging, Auth, Admin modules. Validate patterns at scale.
**Delivers:** All aggregates using AuditableAggregateRoot or IConcurrencyToken, child entities using Entity base, complex queries using Specifications, enums with behavior using SmartEnum.
**Addresses:** Complete feature coverage from FEATURES.md across all modules.
**Research Flag:** Needs phase research for per-module migration strategy, testing approach, and rollback plan.

### Phase Ordering Rationale

- **Phases 15-16 (Foundation + Conventions)** come first because they're non-breaking, low-risk infrastructure changes that establish patterns for subsequent phases. No existing code breaks, pure additive changes.
- **Phase 17 (Result Pattern)** requires architectural decision (ADR) and pilot validation before broader adoption, so it comes after stable foundation exists. Pilot with 1-2 commands, evaluate developer experience, then decide on broader rollout.
- **Phase 18 (Enumeration)** is a breaking migration requiring frontend coordination, deferred until non-breaking patterns proven. Selective migration (only enums with behavior like OrderStatus) rather than bulk replacement.
- **Phase 19 (Specification)** is highest complexity pattern, deferred until simpler building blocks established. Adopt selectively for complex catalog/ordering queries, skip for simple CRUD.
- **Phase 20 (Source Generators)** is optional build-time optimization, deferred until manual patterns validated and 20+ StronglyTypedId types justify complexity.
- **Phase 21 (Adoption)** applies all patterns across 7 feature modules, validating at scale after individual patterns proven in earlier phases.

**Critical dependencies:**
- Phase 16 depends on Phase 15 (conventions need interfaces/base classes)
- Phase 18 depends on Phase 17 (Enumeration errors benefit from Result pattern)
- Phase 19 can run parallel to Phase 18 (independent patterns)
- Phase 21 depends on all previous phases (applies validated patterns)

### Research Flags

**Phases needing deeper research during planning:**
- **Phase 17 (Result Pattern):** Needs ADR creation, error handling boundary definition, pilot command selection criteria, MediatR pipeline behavior registration strategy
- **Phase 19 (Specification Pattern):** Needs specification selection criteria (when to use vs inline LINQ), query candidate identification across modules, testability patterns, performance testing approach
- **Phase 21 (Adoption):** Needs per-module migration strategy, testing approach for 7 modules, rollback plan if patterns don't scale

**Phases with standard patterns (skip research-phase):**
- **Phase 15 (Foundation):** Well-documented EF Core interceptor patterns, standard entity base class implementations
- **Phase 16 (Conventions):** EF Core model conventions are standard .NET feature with official docs
- **Phase 18 (Enumeration):** Library-driven (Ardalis.SmartEnum), standard value converter patterns
- **Phase 20 (Source Generators):** Standard Meziantou generator with comprehensive docs, pilot-first approach validates before broader use

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All libraries are .NET ecosystem standards with official docs, active maintenance, and production usage. FluentResults (4.0.0), Ardalis.SmartEnum (8.2.0), Ardalis.Specification (9.3.1), Meziantou.StronglyTypedId (2.3.11) all verified .NET 10 compatible. Zero deprecated or beta dependencies. |
| Features | HIGH | Feature landscape based on official Microsoft DDD guidance, Enterprise Craftsmanship patterns, and verified with current codebase structure. All six table stakes features identified in existing code gaps (child entities lack base, audit is manual, concurrency is manual per aggregate). |
| Architecture | HIGH | Integration points verified against existing codebase (DomainEventInterceptor at line 159-174, ValidationBehavior exists, StronglyTypedId pattern in use). Additive approach proven via explicit opt-in mechanisms (marker interfaces, inheritance). Data flow changes documented with before/after examples. |
| Pitfalls | HIGH | All eight critical pitfalls sourced from official docs (EF Core inheritance, PostgreSQL xmin docs), real-world issues (GitHub issues on StronglyTypedId, Specification N+1 patterns), and architectural best practices (Result/Exception mixing anti-pattern). Prevention strategies tied to specific phase tasks. |

**Overall confidence:** HIGH

All research areas backed by official documentation (Microsoft Learn EF Core, .NET architecture guides), established library documentation (Ardalis, Meziantou), and verified against existing MicroCommerce codebase patterns. No speculative recommendations or untested patterns.

### Gaps to Address

Minor gaps requiring validation during implementation, none blocking roadmap creation:

- **User Tracking (CreatedBy/ModifiedBy):** Research identified interfaces (IUserCreatable, IUserModifiable) but deferred implementation details. Handle during Phase 15 planning: decide if user tracking is needed, if so implement IHttpContextAccessor integration in AuditInterceptor.
- **SmartEnum vs Enumeration Base Class:** Both Ardalis.SmartEnum (library) and custom Enumeration base (hand-rolled) are viable. Recommend Ardalis for consistency with other Ardalis packages (GuardClauses, Specification), but validate during Phase 18 planning based on team preference.
- **Specification Repository Integration:** Research shows Specification pattern works with both direct DbContext access (current MicroCommerce approach) and repository abstraction. Phase 19 planning should confirm continuing DbContext-direct pattern vs introducing repository layer.
- **Source Generator Choice:** Meziantou.Framework.StronglyTypedId recommended over andrewlock/StronglyTypedId based on stability, but Phase 20 planning should validate converter generation meets all needs (EF Core, JSON, TypeConverter, OpenAPI).

## Sources

### Primary (HIGH confidence)

**Official Microsoft Documentation:**
- [Microsoft Learn - Seedwork DDD base classes](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/seedwork-domain-model-base-classes-interfaces)
- [Microsoft Learn - Enumeration classes over enum types](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types)
- [Microsoft Learn - EF Core Inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [Microsoft Learn - EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [Microsoft Learn - EF Core Concurrency Handling](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [Microsoft Learn - EF Core Value Conversions](https://learn.microsoft.com/en/ef/core/modeling/value-conversions)

**Library Documentation:**
- [FluentResults GitHub Repository](https://github.com/altmann/FluentResults)
- [Ardalis.SmartEnum GitHub Repository](https://github.com/ardalis/SmartEnum) and [official docs](https://www.nuget.org/packages/Ardalis.SmartEnum/)
- [Ardalis.Specification GitHub Repository](https://github.com/ardalis/Specification) and [official docs](http://specification.ardalis.com/)
- [Meziantou.Framework.StronglyTypedId](https://github.com/meziantou/Meziantou.Framework) and [blog post](https://www.meziantou.net/strongly-typed-ids-with-csharp-source-generators.htm)

**EF Core Integration:**
- [Npgsql - Concurrency Tokens with xmin](https://www.npgsql.org/efcore/modeling/concurrency.html)
- [Npgsql - Enum Type Mapping](https://www.npgsql.org/efcore/mapping/enum.html)

### Secondary (MEDIUM confidence)

**DDD Patterns & Best Practices:**
- [Enterprise Craftsmanship - Entity Base Class](https://enterprisecraftsmanship.com/posts/entity-base-class/)
- [Enterprise Craftsmanship - Specification Pattern C# Implementation](https://enterprisecraftsmanship.com/posts/specification-pattern-c-implementation/)
- [Medium - Clean DDD Lessons: Audit Metadata](https://medium.com/unil-ci-software-engineering/clean-ddd-lessons-audit-metadata-for-domain-entities-5935a5c6db5b)
- [ByteAether - Building Enterprise Data Access Layer: Automated Auditing](https://byteaether.github.io/2025/building-an-enterprise-data-access-layer-automated-auditing/)

**Result Pattern Implementation:**
- [Milan Jovanovic - Functional Error Handling with Result Pattern](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)
- [NikolaTech - Result Pattern in .NET](https://www.nikolatech.net/blogs/result-pattern-manage-errors-in-dotnet)
- [Andrew Lock - Is Result Pattern Worth It?](https://andrewlock.net/working-with-the-result-pattern-part-4-is-the-result-pattern-worth-it/)
- [GoatReview - Improving Error Handling with Result Pattern in MediatR](https://goatreview.com/improving-error-handling-result-pattern-mediatr/)

**SmartEnum & Specification Usage:**
- [Code Maze - Improve Enums with SmartEnum Library](https://code-maze.com/csharp-improve-enums-with-the-smartenum-library/)
- [NimblePros - Persisting Smart Enum with EF Core](https://blog.nimblepros.com/blogs/persisting-a-smart-enum-with-entity-framework-core/)
- [NimblePros - Getting Started with Specifications](https://blog.nimblepros.com/blogs/getting-started-with-specifications/)
- [Anton DevTips - Specification Pattern in EF Core](https://antondevtips.com/blog/specification-pattern-in-ef-core-flexible-data-access-without-repositories)

**StronglyTypedId Patterns:**
- [Andrew Lock - Rebuilding StronglyTypedId as Source Generator](https://andrewlock.net/rebuilding-stongly-typed-id-as-a-source-generator-1-0-0-beta-release/)
- [Andrew Lock - Using Strongly-Typed Entity IDs with EF Core](https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-3/)
- [Anton DevTips - Better Entity Identification with Strongly Typed IDs](https://antondevtips.com/blog/a-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)

### Tertiary (LOW confidence, needs validation)

**Performance & Pitfalls:**
- [Medium - Optimistic Locking in .NET](https://medium.com/@imaanmzr/optimistic-locking-in-net-bd677916ef60)
- [Learn EF Core - Concurrency Management](https://www.learnentityframeworkcore.com/concurrency)
- [Digital Drummer - EF Core Audit Fields](https://digitaldrummerj.me/ef-core-audit-columns/)

**Source Generator Edge Cases:**
- [.NET Handbook - Source Generator Best Practices](https://infinum.com/handbook/dotnet/best-practices/source-generators)
- [GitHub - StronglyTypedId CS0436 Warning Issue](https://github.com/andrewlock/StronglyTypedId/issues/38)

---
*Research completed: 2026-02-14*
*Ready for roadmap: yes*
