# Pitfalls Research

**Domain:** DDD Building Blocks for Existing .NET 10 Modular Monolith
**Researched:** 2026-02-14
**Confidence:** HIGH

## Critical Pitfalls

### Pitfall 1: Entity Base Class Breaking Existing EF Core Mappings

**What goes wrong:**
Adding an Entity base class with common properties (Id, CreatedAt, UpdatedAt) to existing aggregates causes EF Core to detect inheritance and attempt to apply TPH (Table-Per-Hierarchy) mapping. This breaks existing configurations, generates migrations that add discriminator columns, and makes existing columns nullable when they shouldn't be.

**Why it happens:**
EF Core automatically scans for inheritance hierarchies. When it detects `Order : Entity<OrderId>`, it assumes this is domain inheritance requiring TPH mapping with a discriminator column, not just a technical base class for code reuse. Developers don't explicitly opt out of inheritance mapping, assuming EF will "just work."

**How to avoid:**
1. Mark the base Entity class as `public abstract class Entity<TId>` and explicitly configure it as **not mapped** using `modelBuilder.Ignore<Entity<TId>>()` in `OnModelCreating`
2. Use property-based inheritance where base class properties are mapped individually per entity type
3. Keep BaseAggregateRoot focused on domain events only, move audit fields to a separate IAuditable interface
4. Document that any new base class must be tested with migration generation before committing

**Warning signs:**
- New migration adds `Discriminator` column to existing tables
- Previously non-nullable columns become nullable after adding base class
- Migration wants to recreate existing tables
- Build succeeds but `Add-Migration` generates unexpected schema changes

**Phase to address:**
Phase 1 (Entity Base Class Design) - Include explicit EF Core inheritance configuration testing in acceptance criteria.

---

### Pitfall 2: Audit Field Interceptor Double-Setting Timestamps

**What goes wrong:**
When adding an audit interceptor to set CreatedAt/UpdatedAt fields, existing entities already set these fields in domain logic (e.g., `Order.Create()` sets `CreatedAt = DateTimeOffset.UtcNow`). The interceptor overwrites the domain-set value, causing timestamp mismatches, or both set it leading to inconsistent timing (domain sets at creation time, interceptor sets at SaveChanges time - potentially milliseconds apart).

**Why it happens:**
Multiple SaveChangesInterceptors execute in registration order. The project already has `DomainEventInterceptor` (lines run after SaveChanges completes). Adding `AuditableEntitiesInterceptor` creates a second interceptor that runs during `SavingChanges`. Developers forget existing entities control their own timestamps and don't check if fields are already set before overwriting.

**How to avoid:**
1. Choose ONE strategy: either domain entities own timestamps OR interceptors own them, never both
2. If using interceptor approach, remove all `CreatedAt = DateTimeOffset.UtcNow` from entity factory methods
3. Use a marker interface `IAuditable` only on entities that DON'T manage their own timestamps
4. In the interceptor, check `if (entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue == default)` before setting
5. Add integration tests that verify timestamp values match expectations

**Warning signs:**
- Timestamps in tests are off by milliseconds when using `DateTimeOffset.UtcNow` in assertions
- CreatedAt and UpdatedAt are identical for new entities (should be different after first update)
- Domain events contain different timestamps than persisted entity
- Aggregate factory methods still set timestamps but interceptor also runs

**Phase to address:**
Phase 2 (Audit Fields & Interceptor) - First task: audit all existing entities, identify which set timestamps, plan migration strategy.

---

### Pitfall 3: PostgreSQL xmin Concurrency Token Loss on Backup/Restore

**What goes wrong:**
The project uses `[Timestamp] public uint Version` mapped to PostgreSQL's `xmin` system column for optimistic concurrency (see OrderConfiguration.cs line 97-98). When the database is backed up and restored, all xmin values reset. Existing loaded entities in application memory have stale xmin values, causing **all subsequent updates to fail with concurrency exceptions** even though no actual concurrent modification occurred.

**Why it happens:**
PostgreSQL's xmin is a transaction ID, not a persistent version number. Backup/restore tools don't preserve transaction IDs. In production, backup/restore operations for blue-green deployments or disaster recovery will reset xmin, but the application doesn't detect this scenario.

**How to avoid:**
1. Add an explicit `Version` column alongside xmin: `public long Version { get; private set; }` that increments manually
2. Use the explicit Version for business logic, keep xmin only as DB-level safety net
3. Document that xmin is unreliable across backup/restore boundaries
4. For entities where concurrency is critical (Order, StockItem), migrate to explicit version column before adding new concurrency features
5. Add alerting when concurrency exception rate spikes (may indicate backup/restore occurred)

**Warning signs:**
- Sudden spike in `DbUpdateConcurrencyException` after database maintenance
- All updates fail after restoring from backup, even with no concurrent users
- Version property shows unexpected values in logs after environment promotion
- Local dev database restore breaks all update operations

**Phase to address:**
Phase 3 (Concurrency Base & Version Strategy) - First task must evaluate xmin reliability vs explicit versioning, include migration plan for existing Order entities.

---

### Pitfall 4: Result Pattern Inconsistency with Existing Exception-Based Flow

**What goes wrong:**
The project currently uses exceptions for error handling: `ValidationBehavior` throws `ValidationException`, domain methods throw `InvalidOperationException` (e.g., Order.MarkAsPaid line 102). Adding Result&lt;T&gt; to **some** operations creates inconsistent error handling where callers don't know if they should check `result.IsFailure` or catch exceptions, leading to uncaught exceptions or ignored Results.

**Why it happens:**
Gradual adoption without a clear boundary. Developers add Result&lt;T&gt; to new features but existing code still throws. MediatR handlers return `Result<OrderDto>` but pipeline behaviors throw exceptions. Frontend expects consistent error response shape but gets mix of 400 (validation exception) and 200 with `{ success: false }`.

**How to avoid:**
1. **Document the boundary**: Commands that fail due to **business rule violations** return Result&lt;T&gt;. Commands that fail due to **invalid input** throw ValidationException (caught by middleware)
2. Update ValidationBehavior to return `Result.Failure()` instead of throwing, or keep it as-is and never use Result for validation failures
3. Create an architectural decision record (ADR): "When to use Result vs Exception"
4. Provide code examples and generator templates for both patterns
5. Add analyzer rule to detect methods that both throw exceptions AND return Result

**Warning signs:**
- Method has `try-catch` block but also checks `if (result.IsFailure)` - redundant patterns
- API endpoint returns 200 OK with error payload instead of 4xx status code
- Some commands throw, some return Result, no clear pattern
- Frontend has inconsistent error handling (`try-catch` in some places, `if (response.isSuccess)` in others)

**Phase to address:**
Phase 4 (Result Pattern Introduction) - Phase 0 task: create ADR and error handling strategy before any Result&lt;T&gt; code is written.

---

### Pitfall 5: Enumeration Class Migration Breaking Existing Enum Serialization

**What goes wrong:**
The project uses C# enums stored as strings (OrderStatus, ProductStatus - see OrderConfiguration line 73-76 `HasConversion<string>()`). Migrating to Enumeration classes requires changing JSON serialization, EF Core value converters, and potentially database values. **Existing API consumers break** because JSON shape changes from `"status": "Submitted"` to `"status": { "value": 1, "name": "Submitted" }`.

**Why it happens:**
Enumeration classes are objects, not primitives. Default JSON serialization produces object graph. Developers write the Enumeration class, update EF Core mappings, but forget to add custom JsonConverter. Frontend and external integrations expect string/int, not object.

**How to avoid:**
1. **Before writing any Enumeration class**: implement and test custom `JsonConverter<Enumeration>` that serializes to string value matching current enum behavior
2. Add integration tests that verify API response JSON shape doesn't change after migration
3. Migrate one enum at a time with full test coverage, don't bulk-replace
4. For each enum, check: domain usage, EF Core mapping, JSON contracts, OpenAPI schema
5. Keep old enum for one release cycle with `[Obsolete]` attribute while Enumeration class runs in parallel

**Warning signs:**
- Frontend displays `[object Object]` instead of status name
- OpenAPI schema shows Enumeration as object type instead of string enum
- API response JSON structure changes after "internal refactoring"
- Swagger UI can't send test requests because enum dropdown is gone
- Database migration wants to change column type from text to composite

**Phase to address:**
Phase 5 (Enumeration Class) - Phase 0: create JsonConverter base class and test with one enum. Phase 1+: migrate enums one at a time, not bulk.

---

### Pitfall 6: Specification Pattern N+1 Queries from IsSatisfiedBy in Loops

**What goes wrong:**
Specification pattern implements `IsSatisfiedBy(TEntity entity)` for in-memory evaluation and `ToExpression()` for database queries. Developers use Specifications correctly in queries (`dbContext.Where(spec.ToExpression())`), but then in business logic call `if (spec.IsSatisfiedBy(entity))` inside a loop over a collection, causing N database roundtrips when entities have lazy-loaded navigation properties accessed by the spec.

**Why it happens:**
Specification reads navigation properties (e.g., `ActiveProductSpecification` checks `product.Category.IsActive`). If entity is loaded without includes, accessing `.Category` triggers lazy load. Calling spec on 100 products = 100 lazy loads. Developer doesn't realize `IsSatisfiedBy` triggers database access because it "looks like" in-memory code.

**How to avoid:**
1. Disable lazy loading globally: `options.UseLazyLoadingProxies(false)` (recommended for CQRS + DDD)
2. Document that Specifications must work on **fully loaded** entities or only be used in query expressions
3. Add analyzer rule: flag `IsSatisfiedBy` called inside loops
4. Provide Specification base class that logs warning when navigation property is null
5. Prefer using Specification only at query boundaries, not in domain logic

**Warning signs:**
- Slow endpoint that loads collections and applies Specifications
- Database profiler shows N identical queries for navigation properties
- Fixing one N+1 issue reveals another in Specification evaluation
- Unit tests pass but integration tests are slow (test DB doesn't reveal N+1 with small datasets)

**Phase to address:**
Phase 6 (Specification Pattern) - Include explicit performance testing with 100+ entity collections in acceptance criteria.

---

### Pitfall 7: Source Generator Build Failures After Multi-Target Framework Change

**What goes wrong:**
Source generators for StronglyTypedId improvements work perfectly in single-target .NET 10 project. Later, when BuildingBlocks.Common is changed to multi-target (`<TargetFrameworks>net10.0;net8.0</TargetFrameworks>`) for compatibility with external tools, the generator runs twice (once per TFM), produces duplicate types, or only runs for one TFM causing build errors in consuming projects.

**Why it happens:**
Source generators execute per target framework. Multi-targeting means generator runs in multiple contexts. If generator isn't idempotent or doesn't check for existing types, it produces conflicts. MSBuild restores and builds for each TFM, but source generator output may not be isolated per TFM.

**How to avoid:**
1. Keep source generator projects single-target .NET 10.0 (they compile to roslyn analyzers, don't need multi-target)
2. If BuildingBlocks.Common must multi-target, move source generators to separate single-target project
3. Add `<IsRoslynComponent>true</IsRoslynComponent>` to generator project to ensure proper isolation
4. Test generators in multi-target scenario **before** production use
5. Document: "Source generator projects must remain single-target"

**Warning signs:**
- Build succeeds locally but fails in CI with "type defined in multiple places"
- Changing TFM order in `<TargetFrameworks>` changes build outcome
- Generated files appear in obj/net10.0/ but not obj/net8.0/ (or vice versa)
- IDE shows red squigglies but build succeeds (or opposite)
- `dotnet build` succeeds but `dotnet build -f net8.0` fails

**Phase to address:**
Phase 7 (Source Generator Setup) - Phase 0: define project structure and TFM strategy before generator implementation.

---

### Pitfall 8: StronglyTypedId JSON Serialization Breaks OpenAPI Schema

**What goes wrong:**
Current StronglyTypedId works in domain (`record ProductId(Guid Value) : StronglyTypedId<Guid>(Value)`). After adding source generator to create custom JsonConverters for StronglyTypedId types, OpenAPI/Swagger schema shows `ProductId` as object `{ value: "guid" }` instead of simple string, breaking auto-generated frontend clients (they generate `ProductId` interface instead of using string alias).

**Why it happens:**
Swashbuckle reads JsonConverter to generate OpenAPI schema. Custom converter doesn't implement `ISchemaFilter` to tell Swashbuckle "serialize as underlying primitive type in schema." Generated TypeScript/C# clients create unnecessary wrapper types.

**How to avoid:**
1. Implement `ISchemaFilter` alongside JsonConverter to map StronglyTypedId to underlying type in OpenAPI
2. Register schema filter: `services.AddSwaggerGen(c => c.SchemaFilter<StronglyTypedIdSchemaFilter>())`
3. Test OpenAPI schema output explicitly: `openapi.json` should show `productId: { type: "string", format: "uuid" }`, not object
4. Test generated client code from OpenAPI schema before releasing
5. Document pattern: all custom serializers need corresponding schema filters

**Warning signs:**
- OpenAPI schema shows nested objects for what should be primitives
- Generated frontend client has `ProductId.value.value` nested access
- Swagger UI requires entering JSON object instead of simple string for ID parameters
- Frontend developers complain about TypeScript types being overly complex
- API accepts `{"value":"..."}` but rejects simple string values

**Phase to address:**
Phase 8 (StronglyTypedId Enhancements) - Task 2 must include OpenAPI schema validation, not just JSON serialization.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Skip explicit EF Core inheritance config | Base class "just works" | Migrations break production, discriminator columns added | Never - always test migration generation |
| Mix Result and Exception error handling | Faster to add Result to one feature | Inconsistent error handling, confusing for team | Only during controlled migration phase with clear end date |
| Use IsSatisfiedBy in loops | Code reads naturally | N+1 queries, performance degradation | Only on in-memory collections, never on DB entities |
| Single audit interceptor for all entities | One place to manage timestamps | Conflicts with entities that own their timestamps | Only if NO existing entities set their own timestamps |
| Keep xmin without explicit version | Minimal code, uses PostgreSQL feature | Breaks on backup/restore, debugging nightmare | Local dev only, never production |
| Lazy load in Specifications | Specifications work without manual includes | N+1 queries, slow at scale | Never - disable lazy loading globally |
| Multi-target BuildingBlocks early | "Future proofs" the library | Source generator issues, build complexity | Only when external .NET 8 integration confirmed |
| Default JsonConverter without SchemaFilter | JSON serialization works | OpenAPI clients broken, API consumer pain | Local dev only, never for public API |

## Integration Gotchas

Common mistakes when connecting to external services.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| EF Core + DomainEventInterceptor | Adding second interceptor without testing execution order | Document interceptor execution order, test that both run |
| MediatR + ValidationBehavior + Result | Behavior throws, handler returns Result - inconsistent | Choose: throw everywhere OR return Result everywhere |
| PostgreSQL xmin + EF Core migrations | Assuming xmin persists across backup/restore | Document limitations, add explicit version column for critical entities |
| MassTransit + Domain Events | Publishing before SaveChanges commits (existing interceptor uses SavedChangesAsync correctly) | Keep using SavedChangesAsync, never SavingChanges for publish |
| Swashbuckle + JsonConverter | Converter works but schema breaks | Always add ISchemaFilter when adding custom JsonConverter |
| Source Generators + Multi-TFM | Generator runs per TFM, creates duplicates | Keep generator projects single-target .NET 10 |
| Npgsql + Enumeration classes | Trying to use MapEnum with Enumeration class (not supported) | Use HasConversion<string> for Enumeration, MapEnum only for C# enums |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| IsSatisfiedBy in loops over lazy entities | Slow endpoints, N SELECT queries | Disable lazy loading, use AsNoTracking | 100+ entities |
| Loading all entities then applying Specification | Works in dev, slow in prod | Use ToExpression() in Where clause | 1000+ rows |
| Multiple SaveChangesInterceptors doing queries | SaveChanges becomes slow | Interceptors should only modify tracked entities | 10+ interceptors |
| Enumeration.GetAll() called repeatedly | No caching, recreates instances | Cache in static field or use source generator | Called in tight loops |
| Result.Failure() with stack trace capture | Useful for debugging, allocates | Disable in production or make opt-in | High-frequency operations |
| Specification.And() chains creating complex SQL | Readable code, terrible query plans | Limit And chains to 3-4, prefer specialized specs | 5+ And chains |
| Domain events published individually | Works with 1-2 events, slow with many | Batch publish (existing interceptor does this correctly) | 10+ events per aggregate |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Entity Base Class:** Added class and updated entities, but forgot to test `Add-Migration` - will break in CI
- [ ] **Audit Interceptor:** Interceptor registered, but existing entities still set timestamps - creates conflicts
- [ ] **Result Pattern:** Returned Result from handler, but pipeline behavior still throws - inconsistent
- [ ] **Enumeration Class:** Created class and EF mapping, but no JsonConverter - API JSON breaks
- [ ] **Specification:** Implemented IsSatisfiedBy, but used in loop over DB entities - N+1 queries
- [ ] **Source Generator:** Works locally, but not tested with multi-target - CI fails
- [ ] **StronglyTypedId JsonConverter:** JSON works, but OpenAPI schema wrong - client generation breaks
- [ ] **Concurrency:** Added Version property, but didn't populate from xmin - all updates fail
- [ ] **Migration:** Created migration, but didn't test on copy of production data - data loss on deploy
- [ ] **Integration Test:** Tests pass, but use in-memory collection not real DB - Specification N+1 not caught

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Entity base added discriminator column | HIGH | Revert migration, add `modelBuilder.Ignore<Entity>()`, generate new migration |
| Audit interceptor overwrites domain timestamps | MEDIUM | Change interceptor to check for default value before setting, OR remove domain logic |
| xmin lost after backup/restore | HIGH | Add explicit Version column, backfill from xmin, update all concurrency checks |
| Result/Exception mixing breaks error handling | MEDIUM | Create ADR, choose one approach, refactor over 2-3 sprints |
| Enumeration breaks JSON serialization | LOW | Add JsonConverter, test API contracts, deploy hotfix |
| Specification causes N+1 | LOW | Add .Include() to query or disable lazy loading |
| Source generator build breaks | HIGH | Move to single-target project, regenerate, test all consuming projects |
| StronglyTypedId breaks OpenAPI | MEDIUM | Add SchemaFilter, regenerate OpenAPI, update client SDKs |
| Multi-interceptor execution order wrong | MEDIUM | Document order, reorder registrations, test with integration test |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Entity base discriminator columns | Phase 1 Task 0 | Generate migration after base class, ensure no discriminator |
| Audit interceptor timestamp conflicts | Phase 2 Task 0 | Audit existing entities, decide single strategy |
| xmin backup/restore failure | Phase 3 Task 0 | Test backup/restore cycle, verify version still works |
| Result/Exception inconsistency | Phase 4 Task 0 | Create ADR before any Result code |
| Enumeration JSON serialization | Phase 5 Task 0 | Create JsonConverter base, test one enum |
| Specification N+1 queries | Phase 6 Task Final | Load test with 100+ entities, check query count |
| Source generator multi-TFM duplication | Phase 7 Task 0 | Define single-target strategy before generator code |
| StronglyTypedId OpenAPI schema | Phase 8 Task 2 | Validate openapi.json schema, test client generation |

## Sources

### EF Core Inheritance & Mapping
- [Inheritance - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [EF Core Inheritance - Learn About TPC, TPH, and TPT Pattern](https://entityframeworkcore.com/model-inheritance)
- [Working with Inheritance in Entity Framework Core](https://blog.devart.com/how-to-work-with-inheritance-in-entity-framework-core.html)

### SaveChangesInterceptor & Audit
- [Interceptors - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [EF Core Interceptors: SaveChangesInterceptor for Auditing Entities in .NET 8 Microservices](https://mehmetozkaya.medium.com/ef-core-interceptors-savechangesinterceptor-for-auditing-entities-in-net-8-microservices-6923190a03b9)
- [How To Use EF Core Interceptors](https://www.milanjovanovic.tech/blog/how-to-use-ef-core-interceptors)

### PostgreSQL xmin Concurrency
- [Concurrency Tokens | Npgsql Documentation](https://www.npgsql.org/efcore/modeling/concurrency.html)
- [Handling Concurrency Conflicts - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [How to use xmin as version control for records · Issue #2778 · npgsql/efcore.pg](https://github.com/npgsql/efcore.pg/issues/2778)

### Result Pattern
- [Is the result pattern worth it?: Working with the result pattern - Part 4](https://andrewlock.net/working-with-the-result-pattern-part-4-is-the-result-pattern-worth-it/)
- [The Result Pattern in C#: A comprehensive guide](https://www.linkedin.com/pulse/result-pattern-c-comprehensive-guide-andre-baltieri-wieuf)

### Enumeration Classes
- [Enum Type Mapping | Npgsql Documentation](https://www.npgsql.org/efcore/mapping/enum.html)
- [Enum mapping with v9 and ordering · Issue #3390 · npgsql/efcore.pg](https://github.com/npgsql/efcore.pg/issues/3390)
- [Value Conversions - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

### Specification Pattern
- [Specification pattern: C# implementation · Enterprise Craftsmanship](https://enterprisecraftsmanship.com/posts/specification-pattern-c-implementation/)
- [Specification Pattern in Java: Enhancing Business Rules with Decoupled Logic](https://java-design-patterns.com/patterns/specification/)

### Source Generators
- [.NET Handbook | Best Practices / Source Generators](https://infinum.com/handbook/dotnet/best-practices/source-generators)
- [Errors and warnings associated with source generators - C# reference | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/source-generator-errors)
- [.NET 10 based source generator and false IDE unresolved symbol errors · Issue #81475 · dotnet/roslyn](https://github.com/dotnet/roslyn/issues/81475)

### StronglyTypedId
- [CS0436 Warning · Issue #38 · andrewlock/StronglyTypedId](https://github.com/andrewlock/StronglyTypedId/issues/38)
- [Exception is thrown when deserializing nullable strongly-typed id · Issue #36 · andrewlock/StronglyTypedId](https://github.com/andrewlock/StronglyTypedId/issues/36)
- [GitHub - andrewlock/StronglyTypedId](https://github.com/andrewlock/StronglyTypedId)

### MediatR Error Handling
- [Improving Error Handling with the Result Pattern in MediatR](https://goatreview.com/improving-error-handling-result-pattern-mediatr/)
- [MediatR Response: Should the Request Handler Return Exceptions?](https://medium.com/sharpassembly/mediatr-response-should-the-request-handler-return-exceptions-8a7928a7c572)
- [Global Exception Handling for MediatR Requests - Code Maze](https://code-maze.com/csharp-global-exception-handling-for-mediatr-requests/)

### DDD Patterns
- [DDD Modelling - Aggregates vs Entities: A Practical Guide](https://www.dandoescode.com/blog/ddd-modelling-aggregates-vs-entities)
- [Modeling Aggregates with DDD and Entity Framework](https://kalele.io/modeling-aggregates-with-ddd-and-entity-framework/)
- [Creating Domain-Driven Design entity classes with Entity Framework Core](https://www.thereformedprogrammer.net/creating-domain-driven-design-entity-classes-with-entity-framework-core/)

---
*Pitfalls research for: DDD Building Blocks in .NET 10 Modular Monolith*
*Researched: 2026-02-14*
