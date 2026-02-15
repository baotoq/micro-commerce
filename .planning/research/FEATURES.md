# Feature Landscape: DDD Building Blocks

**Domain:** .NET 10 modular monolith e-commerce platform - DDD infrastructure improvements
**Researched:** 2026-02-14
**Confidence:** HIGH (verified with official docs, EF Core patterns, OSS library standards)

## Table Stakes

Features expected in a comprehensive DDD building blocks library. Missing = developers write boilerplate manually or inconsistently.

| Feature | Why Expected | Complexity | Dependencies | Notes |
|---------|--------------|------------|--------------|-------|
| **Entity Base Class** | Standardizes ID handling, equality, child entity patterns | Low | Existing BaseAggregateRoot | Already have for aggregates, need for child entities (CartItem, OrderItem) |
| **Audit Field Interfaces** | Automatic CreatedAt/ModifiedAt/CreatedBy/ModifiedBy tracking across all entities | Medium | Entity base, SaveChangesInterceptor | Currently manual per aggregate, industry standard is interface-based automation |
| **Optimistic Concurrency Base** | Standardizes RowVersion pattern for aggregates needing concurrency control | Low | Entity base | Currently manual [Timestamp] on Order.Version, should be opt-in base class/interface |
| **StronglyTypedId Converters** | EF Core, System.Text.Json, TypeConverter support for all ID types | Medium | Source generators | Currently manual inheritance from StronglyTypedId<Guid>, need auto TypeConverter/JsonConverter |
| **Specification Pattern** | Reusable, composable query logic instead of repository method explosion | High | None (standalone pattern) | Complex queries manually coded, Specification enables IRepository.Get(spec) |
| **Enumeration/SmartEnum** | Enums with behavior, encapsulation, type safety, EF Core persistence | Low | Ardalis.SmartEnum or custom base | Currently plain enums (OrderStatus, ProductStatus), no behavior or centralized logic |

## Differentiators

Features not universally expected but provide significant value when building domain-rich applications.

| Feature | Value Proposition | Complexity | Dependencies | Notes |
|---------|-------------------|------------|--------------|-------|
| **Result<T> Monad** | Railway-oriented error handling, eliminates exception-driven flow for business logic failures | Medium | None (standalone) | Replaces `throw DomainException`, enables `Result.Success(order)` or `Result.Failure("Stock unavailable")` |
| **Guard Clauses Library** | Fluent, expressive input validation (already using Ardalis.GuardClauses) | Low | Already in use | KEEP existing usage, table stake for domain validation |
| **Domain Event Dispatcher** | Automatic in-process event handling before SaveChanges (MediatR notifications) | Medium | MediatR, DomainEventInterceptor | Already have DomainEventInterceptor + MassTransit for out-of-process, could add in-process handlers |
| **Soft Delete Interface** | ISoftDeletable + interceptor for logical deletion pattern | Low | SaveChangesInterceptor | Product.Archive() is manual, could be interface-driven with global query filter |
| **Tenant ID Interface** | Multi-tenancy support via IHasTenant + global query filters | Medium | SaveChangesInterceptor, EF query filters | Not needed for current single-tenant e-commerce, but common in SaaS DDD apps |
| **Outbox Pattern Support** | Transactional outbox for reliable domain event publishing | High | Dedicated outbox table, background processor | MassTransit handles this, would duplicate, mark as ANTI-FEATURE unless replacing MassTransit |

## Anti-Features

Features to explicitly NOT build, with rationale.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| **Generic Repository** | Leaky abstraction, EF Core DbContext already is unit-of-work + repository | Use DbContext directly per feature (already doing this) |
| **Auto-mapper for Aggregates** | Domain logic should be explicit, automapping hides business rules | Keep explicit factory methods (Order.Create, Product.Update) |
| **Specification Builder UI** | Over-engineering, adds indirection without value | Write Specifications manually for complex queries only |
| **Custom ORM Wrapper** | EF Core is mature, wrapping adds complexity without benefit | Use EF Core directly with building block extensions |
| **Domain Services Base Class** | Domain services are stateless application patterns, no shared base needed | Keep as standalone classes (already doing this) |
| **Transaction Script Helpers** | DDD is about rich domain models, transaction scripts are procedural anti-pattern | Enforce aggregate encapsulation instead |

## Feature Dependencies

```
Audit Interfaces → Entity Base Class → SaveChangesInterceptor (already exists)
StronglyTypedId Converters → StronglyTypedId<T> (already exists)
Specification Pattern → (standalone, no dependencies)
SmartEnum → (standalone OR Ardalis.SmartEnum library)
Result<T> → (standalone, no dependencies)
Soft Delete → Entity Base OR Interface → SaveChangesInterceptor
Optimistic Concurrency → Entity Base OR Interface
```

## MVP Recommendation

**Phase 1: Foundations (Low-hanging fruit, high impact)**
1. **Audit Field Interfaces** - ICreatable, IModifiable, IUserCreatable, IUserModifiable with SaveChangesInterceptor
   - Eliminates repetitive `CreatedAt = DateTimeOffset.UtcNow` in every factory method
   - Enables user tracking (CreatedBy/ModifiedBy) via IHttpContextAccessor
   - Complexity: Medium (interceptor already exists, need interfaces + user resolution)

2. **Optimistic Concurrency Interface** - IConcurrent with RowVersion property + EF config helper
   - Currently only Order has Version, but Cart, Product, StockItem also need concurrency
   - Complexity: Low (just standardize existing pattern)

3. **StronglyTypedId Source Generators** - Add Meziantou.Framework.StronglyTypedId or andrewlock/StronglyTypedId
   - Auto-generates TypeConverter, JsonConverter, EF ValueConverter per ID type
   - Eliminates manual EF configuration for 15+ strongly typed IDs
   - Complexity: Medium (NuGet + attribute decoration)

**Phase 2: Behavior Enrichment**
4. **Enumeration Classes** - Replace plain enums with Ardalis.SmartEnum
   - OrderStatus becomes SmartEnum with behavior: `OrderStatus.Submitted.CanTransitionTo(OrderStatus.Paid)`
   - ProductStatus gets display names, descriptions, transition rules
   - Complexity: Low (library handles persistence, just define enums)

5. **Result<T> Pattern** - Add Ardalis.Result or FluentResults
   - Replace exceptions in commands: `Result<Order> PlaceOrder(...)` instead of throwing
   - Enables `return Result.Invalid(validationErrors)` at application layer
   - Complexity: Medium (refactor command handlers to return Result)

**Phase 3: Query Patterns**
6. **Specification Pattern** - Custom implementation based on Ardalis.Specification patterns
   - Complex catalog queries: `ProductsInCategoryWithReviewsSpec`, `PublishedProductsWithStockSpec`
   - Reusable across queries and tests
   - Complexity: High (requires generic repository abstraction or DbSet extensions)

**Defer (Not needed for current scope)**
- Soft Delete Interface: Product.Archive() is explicit domain logic, not infrastructure concern
- Tenant ID: Single-tenant application, premature
- Outbox Pattern: MassTransit handles this
- Domain Event Dispatcher (in-process): Domain events already work via interceptor + MassTransit

## Implementation Notes

### Audit Fields
**Interfaces:**
```csharp
public interface ICreatable { DateTimeOffset CreatedAt { get; set; } }
public interface IModifiable { DateTimeOffset ModifiedAt { get; set; } }
public interface IUserCreatable : ICreatable { string CreatedBy { get; set; } }
public interface IUserModifiable : IModifiable { string ModifiedBy { get; set; } }
```

**Interceptor pattern (SaveChangesInterceptor):**
```csharp
public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
{
    var entries = eventData.Context.ChangeTracker.Entries()
        .Where(e => e.State is EntityState.Added or EntityState.Modified);

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added && entry.Entity is ICreatable creatable)
            creatable.CreatedAt = DateTimeOffset.UtcNow;

        if (entry.State == EntityState.Modified && entry.Entity is IModifiable modifiable)
            modifiable.ModifiedAt = DateTimeOffset.UtcNow;
    }
}
```

### Optimistic Concurrency
**Option 1: Interface-based**
```csharp
public interface IConcurrent { uint Version { get; set; } }
```
Then use EF model configuration convention to apply [Timestamp] globally.

**Option 2: Base class** (for aggregates already extending BaseAggregateRoot)
```csharp
public abstract class ConcurrentAggregateRoot<TId> : BaseAggregateRoot<TId>, IConcurrent
{
    [Timestamp] public uint Version { get; set; }
}
```

### StronglyTypedId Converters
**Library choice:** Meziantou.Framework.StronglyTypedId (active, .NET 10 compatible)

**Usage:**
```csharp
[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson, converters: StronglyTypedIdConverter.TypeConverter | StronglyTypedIdConverter.EfCoreValueConverter)]
public readonly partial record struct ProductId(Guid Value);
```

Source generator creates TypeConverter, JsonConverter, EF ValueConverter automatically.

### Enumeration Classes
**Library choice:** Ardalis.SmartEnum (8.2.0+, industry standard)

**Migration example:**
```csharp
// Before: plain enum
public enum OrderStatus { Submitted, Paid, Confirmed }

// After: SmartEnum with behavior
public class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Submitted = new(nameof(Submitted), 1);
    public static readonly OrderStatus Paid = new(nameof(Paid), 2);
    public static readonly OrderStatus Confirmed = new(nameof(Confirmed), 3);

    private OrderStatus(string name, int value) : base(name, value) {}

    public bool CanTransitionTo(OrderStatus next) => next.Value == Value + 1; // Domain logic
}
```

EF Core persistence via `Ardalis.SmartEnum.EFCore` package (value converter).

### Result Pattern
**Library choice:** Ardalis.Result (for consistency with Ardalis.GuardClauses, Ardalis.SmartEnum)

**Usage in command handlers:**
```csharp
// Before: throw exception
public async Task<Order> Handle(PlaceOrderCommand request, CancellationToken ct)
{
    if (cart.Items.Count == 0)
        throw new InvalidOperationException("Cart is empty");
    return order;
}

// After: Result<T>
public async Task<Result<Order>> Handle(PlaceOrderCommand request, CancellationToken ct)
{
    if (cart.Items.Count == 0)
        return Result.Invalid(new ValidationError("Cart cannot be empty"));
    return Result.Success(order);
}
```

ASP.NET Core integration via `Ardalis.Result.AspNetCore` (auto-maps to IResult/IActionResult).

### Specification Pattern
**Custom implementation** based on Ardalis.Specification patterns but adapted for EF Core DbSet extensions.

**Interface:**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
}
```

**Usage:**
```csharp
public class PublishedProductsInCategorySpec : Specification<Product>
{
    public PublishedProductsInCategorySpec(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Published)
             .OrderByDescending(p => p.CreatedAt);
    }
}

// In query handler
var products = await dbContext.Products.ApplySpecification(spec).ToListAsync();
```

## Complexity Assessment

| Feature | Lines of Code | Integration Points | Risk Level |
|---------|--------------|-------------------|------------|
| Audit Interfaces | ~50 (interfaces + interceptor) | SaveChangesInterceptor, all aggregates | Low (existing pattern) |
| Concurrency Base | ~20 (interface + config) | EF model builder | Low (existing pattern) |
| StronglyTypedId Generators | ~10 (attributes) | All 15+ ID types | Low (library-driven) |
| SmartEnum | ~200 (2 enum migrations) | OrderStatus, ProductStatus, EF configs | Medium (data migration) |
| Result<T> | ~300 (command handler refactor) | MediatR pipeline, ASP.NET endpoints | Medium (changes error handling) |
| Specification Pattern | ~150 (base + extensions) | Catalog/Ordering queries | High (new abstraction) |

## Migration Strategy

**Backward Compatibility:**
- Audit interfaces: Add to aggregates progressively, old aggregates keep manual CreatedAt until migrated
- Concurrency: Opt-in interface, existing Order.Version unchanged
- StronglyTypedId: Backward compatible (same record struct, just adds converters)
- SmartEnum: Breaking change for enums, requires data migration + frontend DTO updates
- Result<T>: Opt-in per command, existing exception-based handlers unchanged
- Specification: Additive, existing LINQ queries unchanged

**Rollout Order:**
1. Non-breaking (StronglyTypedId generators, Audit interfaces, Concurrency interface)
2. Opt-in additions (Result<T> for new commands, Specifications for new complex queries)
3. Breaking migrations (SmartEnum replacing plain enums - coordinate with frontend)

## Sources

### Official Documentation
- [Microsoft - Seedwork DDD base classes](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/seedwork-domain-model-base-classes-interfaces)
- [Microsoft - Enumeration classes over enum types](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types)
- [Microsoft - EF Core Concurrency Handling](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [Microsoft - EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)

### DDD Patterns
- [Enterprise Craftsmanship - Entity Base Class](https://enterprisecraftsmanship.com/posts/entity-base-class/)
- [Enterprise Craftsmanship - Specification Pattern C# Implementation](https://enterprisecraftsmanship.com/posts/specification-pattern-c-implementation/)
- [Medium - Clean DDD Lessons: Audit Metadata](https://medium.com/unil-ci-software-engineering/clean-ddd-lessons-audit-metadata-for-domain-entities-5935a5c6db5b)
- [ByteAether - Automated Auditing with EF Core](https://byteaether.github.io/2025/building-an-enterprise-data-access-layer-automated-auditing/)
- [Milan Jovanovic - EF Core Interceptors](https://www.milanjovanovic.tech/blog/how-to-use-ef-core-interceptors)

### Libraries & Implementations
- [Ardalis SmartEnum GitHub](https://github.com/ardalis/SmartEnum)
- [Ardalis Result GitHub](https://github.com/ardalis/Result)
- [Andrew Lock - StronglyTypedId Source Generator](https://github.com/andrewlock/StronglyTypedId)
- [Meziantou StronglyTypedId NuGet](https://www.nuget.org/packages/Meziantou.Framework.StronglyTypedId)
- [FluentResults GitHub](https://github.com/altmann/FluentResults)

### Result Pattern Comparisons
- [NikolaTech - Result Pattern in .NET](https://www.nikolatech.net/blogs/result-pattern-manage-errors-in-dotnet)
- [Anton DevTips - Replace Exceptions with Result Pattern](https://antondevtips.com/blog/how-to-replace-exceptions-with-result-pattern-in-dotnet)

### Specification Pattern
- [DevIQ - Specification Pattern](https://deviq.com/design-patterns/specification-pattern/)
- [Ardalis Specification - Use with Repository Pattern](https://specification.ardalis.com/usage/use-specification-repository-pattern.html)
- [Medium - Specification Pattern in DDD .NET Core](https://medium.com/@cizu64/the-query-specification-pattern-in-ddd-net-core-25f1ec580f32)

### Concurrency & Audit
- [Learn EF Core - Concurrency Management](https://www.learnentityframeworkcore.com/concurrency)
- [Medium - Optimistic Locking in .NET](https://medium.com/@imaanmzr/optimistic-locking-in-net-bd677916ef60)
- [Medium - Audit Automation with EF Core](https://medium.com/@bananicabananica/audit-automation-with-ef-core-2f629fb77523)
- [Digital Drummer - EF Core Audit Fields](https://digitaldrummerj.me/ef-core-audit-columns/)
