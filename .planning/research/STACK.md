# Stack Research: DDD Building Blocks Improvements

**Domain:** Domain-Driven Design Building Blocks for .NET 10 Modular Monolith
**Researched:** 2026-02-14
**Confidence:** HIGH

## Recommended Stack

### Core Building Block Libraries

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| FluentResults | 4.0.0 | Result<T> pattern implementation | Mature library with .NET 8/9 support, explicit .NET Standard 2.0/2.1 targeting, fluent API, supports multiple errors per result. Better for complex error scenarios with rich metadata. |
| Ardalis.SmartEnum | 8.2.0 | Strongly-typed enumeration replacement | Standard choice in .NET DDD community, created by respected architect Steve Smith. Supports rich behavior, custom properties, and type-safe comparisons. |
| Ardalis.Specification | 9.3.1 | Query specification pattern | De facto standard for DDD specifications in .NET, integrates seamlessly with EF Core, promotes query reuse and testability. Part of Microsoft's eShopOnWeb reference architecture. |
| Meziantou.Framework.StronglyTypedId | 2.3.11 | Source generator for strongly-typed IDs | Most comprehensive source generator for StronglyTypedId pattern. Zero dependencies, generates all needed converters (System.Text.Json, Newtonsoft.Json, MongoDB BSON, TypeConverter). Active maintenance (updated Jan 2026). |

### Supporting EF Core Integration

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Ardalis.Specification.EntityFrameworkCore | 9.3.1 | EF Core adapter for specifications | Use with any repository pattern implementation that needs specification support. Requires EF Core 8.0.19+ or 9.0.8+. |
| Ardalis.SmartEnum.EFCore | 8.2.0 | EF Core value converter for SmartEnum | Use when persisting SmartEnum to database. Leverages EF Core 6+ pre-convention model configuration for automatic conversion setup. Requires EF Core 7.0.13+. |
| Microsoft.EntityFrameworkCore | 10.0.3 | ORM for database access | Already in use. EF Core 10 is the matching version for .NET 10. Use for all persistence operations. |

### Framework Dependencies (Already Present)

| Framework | Version | Purpose | Notes |
|-----------|---------|---------|-------|
| .NET | 10.0 | Runtime | Already validated capability |
| MediatR | 13.1.0 | CQRS command/query pipeline | Already in use, Result<T> integrates well with MediatR handlers |
| Ardalis.GuardClauses | 5.0.0 | Input validation | Already in use, complements Result<T> for immediate failures |

## Installation

```bash
# Core DDD Building Blocks
dotnet add package FluentResults --version 4.0.0
dotnet add package Ardalis.SmartEnum --version 8.2.0
dotnet add package Ardalis.Specification --version 9.3.1

# EF Core Integration
dotnet add package Ardalis.Specification.EntityFrameworkCore --version 9.3.1
dotnet add package Ardalis.SmartEnum.EFCore --version 8.2.0

# Source Generator (private assets, build-time only)
dotnet add package Meziantou.Framework.StronglyTypedId --version 2.3.11
```

Add to BuildingBlocks.Common.csproj:
```xml
<ItemGroup>
  <!-- Result Pattern -->
  <PackageReference Include="FluentResults" Version="4.0.0" />

  <!-- Enumeration Class -->
  <PackageReference Include="Ardalis.SmartEnum" Version="8.2.0" />

  <!-- Specification Pattern -->
  <PackageReference Include="Ardalis.Specification" Version="9.3.1" />

  <!-- Source Generator - Build-time only -->
  <PackageReference Include="Meziantou.Framework.StronglyTypedId" Version="2.3.11">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>

  <!-- Existing dependencies (already present) -->
  <PackageReference Include="Ardalis.GuardClauses" Version="5.0.0" />
  <PackageReference Include="MediatR" Version="13.1.0" />
</ItemGroup>
```

Add to feature modules (Catalog, Cart, etc.) that use specifications:
```xml
<ItemGroup>
  <PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="9.3.1" />
  <PackageReference Include="Ardalis.SmartEnum.EFCore" Version="8.2.0" />
</ItemGroup>
```

## Alternatives Considered

| Category | Recommended | Alternative | When to Use Alternative |
|----------|-------------|-------------|-------------------------|
| Result Pattern | FluentResults | ErrorOr 2.0.1 | Use ErrorOr if you prefer more functional programming style with simpler API. ErrorOr is lighter-weight but less feature-rich. Both support multiple errors. |
| Result Pattern | FluentResults | Custom implementation | Never - reinventing is technical debt. FluentResults is battle-tested and zero-cost abstraction. |
| SmartEnum | Ardalis.SmartEnum | Custom Enumeration base class | Never for new code - Ardalis.SmartEnum is community standard with EF Core integration. |
| Specification | Ardalis.Specification | Custom implementation | Only if you need non-EF Core data sources. Ardalis.Specification has adapters for most scenarios. |
| StronglyTypedId Generator | Meziantou.Framework.StronglyTypedId | StronglyTypedId (andrewlock) 1.0.0-beta08 | Avoid - still in beta after 2+ years. Meziantou is stable, more feature-complete, and actively maintained. |
| StronglyTypedId Generator | Meziantou.Framework.StronglyTypedId | Manual converters | Never - source generators eliminate boilerplate at zero runtime cost. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| andrewlock/StronglyTypedId beta versions | Still in beta (1.0.0-beta08 as of April 2024), no stable 1.0 release after years | Meziantou.Framework.StronglyTypedId 2.3.11 (stable, actively maintained) |
| Custom ValueObject base class | Already obsoleted in codebase, `readonly record struct` is superior in C# 10+ | `readonly record struct` for value objects (already migrated) |
| Exception-driven error handling for domain logic | Expensive, makes control flow unclear, poor for validation scenarios | FluentResults for expected failures, exceptions only for unexpected |
| String-based error messages | Loses type safety, hard to test, no structured metadata | FluentResults with custom Error classes |
| Repository methods returning null for not-found | Ambiguous (null = not found or error?), causes null checks everywhere | Result<T> with explicit NotFound error |
| EF Core shadow properties for audit fields | Invisible in domain model, hard to test, violates DDD principles | Explicit CreatedAt/UpdatedAt properties with SaveChangesInterceptor |

## Stack Patterns by Use Case

### Pattern 1: Entity Base Class with Audit Fields

**Use when:** Every entity needs creation/modification tracking

```csharp
public abstract class AuditableEntity<TId> : BaseAggregateRoot<TId>
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Set by SaveChangesInterceptor, not domain code
    internal void SetCreatedAt(DateTime timestamp) => CreatedAt = timestamp;
    internal void SetUpdatedAt(DateTime timestamp) => UpdatedAt = timestamp;
}
```

**Why:** Keeps audit concerns separate from business logic, enforced via EF Core interceptor.

### Pattern 2: Entity Base Class without Audit (Concurrency Only)

**Use when:** Entity doesn't need timestamps but needs concurrency protection

```csharp
public abstract class ConcurrentEntity<TId> : BaseAggregateRoot<TId>
{
    [Timestamp]
    public byte[] RowVersion { get; private set; }
}
```

**Why:** Optimistic concurrency via PostgreSQL's xmin or SQL Server rowversion, minimal overhead.

### Pattern 3: Result<T> for Command Handlers

**Use when:** Command can fail for business reasons (validation, not found, conflict)

```csharp
public record AddToCartCommand(ProductId ProductId, int Quantity) : IRequest<Result<CartId>>;

internal class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<CartId>>
{
    public async Task<Result<CartId>> Handle(...)
    {
        if (quantity <= 0)
            return Result.Fail<CartId>("Quantity must be positive");

        var product = await _repository.GetByIdAsync(request.ProductId);
        if (product is null)
            return Result.Fail<CartId>($"Product {request.ProductId} not found");

        // ... success path
        return Result.Ok(cart.Id);
    }
}
```

**Why:** Explicit error handling, no exceptions for expected failures, easy to test.

### Pattern 4: Specification Pattern for Complex Queries

**Use when:** Query logic is reused across multiple handlers or needs independent testing

```csharp
public class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec()
    {
        Query.Where(p => !p.IsDeleted && p.StockQuantity > 0);
    }
}

public class ProductsByCategorySpec : Specification<Product>
{
    public ProductsByCategorySpec(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId);
    }
}

// Usage in repository
var spec = new ActiveProductsSpec()
    .And(new ProductsByCategorySpec(categoryId));
var products = await _repository.ListAsync(spec);
```

**Why:** Testable query logic, composable specifications, keeps repositories thin.

### Pattern 5: SmartEnum for Domain-Driven Enumerations

**Use when:** Enumeration needs behavior, validation, or rich metadata

```csharp
public class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Pending = new(nameof(Pending), 1);
    public static readonly OrderStatus Confirmed = new(nameof(Confirmed), 2);
    public static readonly OrderStatus Shipped = new(nameof(Shipped), 3);
    public static readonly OrderStatus Delivered = new(nameof(Delivered), 4);
    public static readonly OrderStatus Cancelled = new(nameof(Cancelled), 5);

    private OrderStatus(string name, int value) : base(name, value) { }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return (this, newStatus) switch
        {
            (var current, var next) when current == next => false,
            (var current, _) when current == Cancelled => false,
            (var current, _) when current == Delivered => false,
            (var _, var next) when next == Cancelled => true,
            (var current, var next) when next.Value == current.Value + 1 => true,
            _ => false
        };
    }
}
```

**Why:** Domain logic stays with domain concept, type-safe, persists as int in database.

### Pattern 6: StronglyTypedId with Source Generator

**Use when:** Defining new aggregate root or entity IDs

```csharp
[StronglyTypedId<Guid>]
public partial struct OrderId { }

// Generator creates:
// - Constructors, properties
// - IEquatable<OrderId>, IComparable<OrderId>
// - System.Text.Json converter
// - Newtonsoft.Json converter
// - TypeConverter for model binding
// - MongoDB BSON serializer (if referenced)
```

**Why:** Zero boilerplate, compile-time generation, all converters included, consistent pattern.

## Version Compatibility

| Package | Requires | Notes |
|---------|----------|-------|
| FluentResults 4.0.0 | .NET Standard 2.0+ OR .NET 8/9 | .NET 10 compatible via .NET Standard 2.0/2.1 |
| Ardalis.SmartEnum 8.2.0 | .NET 6+ OR .NET Standard 2.0 | Explicitly supports .NET 8, forward compatible |
| Ardalis.Specification 9.3.1 | .NET 8+ OR .NET 9+ | Ships with framework-specific builds |
| Ardalis.Specification.EntityFrameworkCore 9.3.1 | EF Core 8.0.19+ (for .NET 8), EF Core 9.0.8+ (for .NET 9) | Works with EF Core 10.0.3 (forward compatible) |
| Ardalis.SmartEnum.EFCore 8.2.0 | EF Core 7.0.13+ | Works with EF Core 10.0.3 |
| Meziantou.Framework.StronglyTypedId 2.3.11 | .NET Standard 2.0+ OR .NET 8+ | Source generator, no runtime dependency |
| ErrorOr 2.0.1 | .NET 6+ OR .NET Standard 2.0 | Alternative to FluentResults |

**All recommended packages are .NET 10 compatible** through either explicit .NET Standard 2.0/2.1 support or forward compatibility from .NET 8/9.

## Integration with Existing Stack

### MediatR Integration

Result<T> integrates naturally with MediatR handlers:

```csharp
// Commands return Result<T> for explicit error handling
public record CreateOrderCommand(...) : IRequest<Result<OrderId>>;

// Queries can return Result<T> or T depending on failure scenarios
public record GetOrderQuery(OrderId Id) : IRequest<Result<Order>>; // Can fail (not found)
public record ListOrdersQuery(...) : IRequest<List<Order>>;        // Can't fail (returns empty)
```

### EF Core Integration

1. **Specifications with DbContext:**

```csharp
public class CatalogDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    // No special configuration needed, Ardalis.Specification.EntityFrameworkCore
    // works via extension methods on DbSet<T>
}
```

2. **SmartEnum Value Conversion (EF Core 6+ pre-convention):**

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.ConfigureSmartEnum(); // Ardalis.SmartEnum.EFCore
}
```

3. **Audit Fields via SaveChangesInterceptor:**

```csharp
public class AuditingInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedAt(now);

            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt(now);
        }
    }
}

// Register in DI
services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(new AuditingInterceptor());
});
```

4. **StronglyTypedId with EF Core (auto-detected):**

Meziantou's generator creates TypeConverter which EF Core uses automatically:

```csharp
[StronglyTypedId<Guid>]
public partial struct ProductId { }

// No manual configuration needed in OnModelCreating
// EF Core detects TypeConverter and applies value conversion
```

### Minimal API Integration

FluentResults works seamlessly with ASP.NET Core:

```csharp
app.MapPost("/cart/items", async (
    AddToCartCommand command,
    IMediator mediator) =>
{
    var result = await mediator.Send(command);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors.Select(e => e.Message));
});
```

Or use FluentResults.Extensions.AspNetCore (optional package) for automatic mapping to ProblemDetails.

## Avoiding Library Bloat

### Principles Applied

1. **Zero Runtime Dependencies:** Meziantou.Framework.StronglyTypedId is build-time only (source generator)
2. **Minimal Transitive Dependencies:** FluentResults has no dependencies; Ardalis packages only depend on what they integrate with
3. **No Overlapping Functionality:** Each library has single responsibility (Result, Enumeration, Specification, ID generation)
4. **Framework-Aligned:** All packages target .NET Standard 2.0 minimum, avoiding framework version conflicts

### Dependency Audit

```
BuildingBlocks.Common dependencies (4 new + 2 existing):
├── FluentResults 4.0.0 (no dependencies)
├── Ardalis.SmartEnum 8.2.0 (no dependencies)
├── Ardalis.Specification 9.3.1 (no dependencies)
├── Meziantou.Framework.StronglyTypedId 2.3.11 (no dependencies, build-time only)
├── Ardalis.GuardClauses 5.0.0 (existing, no dependencies)
└── MediatR 13.1.0 (existing, brings MediatR.Contracts 2.0.1)

Feature module additional dependencies (when using specs/enums):
├── Ardalis.Specification.EntityFrameworkCore 9.3.1
│   ├── Ardalis.Specification 9.3.1 (already in BuildingBlocks.Common)
│   ├── Microsoft.EntityFrameworkCore 8.0.19+ (already referenced)
│   └── Microsoft.EntityFrameworkCore.Relational 8.0.19+ (already referenced)
└── Ardalis.SmartEnum.EFCore 8.2.0
    ├── Ardalis.SmartEnum 8.2.0 (already in BuildingBlocks.Common)
    ├── Microsoft.EntityFrameworkCore 7.0.13+ (already referenced)
    └── System.Text.Json 9.0.0 (framework-provided in .NET 10)

Total new packages: 6
Total new transitive dependencies: 0 (all integrate with existing EF Core)
```

### What We're NOT Adding

- **Audit.EntityFramework.Core:** Too heavyweight (full audit trail), we only need timestamps
- **AutoMapper:** Not needed for DDD (explicit mapping is clearer)
- **Custom ValueObject base class:** Already obsoleted, using `readonly record struct`
- **Custom Result implementation:** FluentResults is mature and well-tested
- **Multiple Result libraries:** Pick one (FluentResults), avoid duplication

## Sources

### Result Pattern
- [FluentResults GitHub Repository](https://github.com/altmann/FluentResults)
- [FluentResults 4.0.0 on NuGet](https://www.nuget.org/packages/FluentResults/)
- [ErrorOr GitHub Repository](https://github.com/amantinband/error-or)
- [ErrorOr 2.0.1 on NuGet](https://www.nuget.org/packages/erroror)
- [Functional Error Handling in .NET With the Result Pattern](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)
- [Manage Errors with Result Pattern in .NET](https://www.nikolatech.net/blogs/result-pattern-manage-errors-in-dotnet)

### SmartEnum
- [Ardalis.SmartEnum GitHub Repository](https://github.com/ardalis/SmartEnum)
- [Ardalis.SmartEnum 8.2.0 on NuGet](https://www.nuget.org/packages/Ardalis.SmartEnum/)
- [Ardalis.SmartEnum.EFCore 8.2.0 on NuGet](https://www.nuget.org/packages/Ardalis.SmartEnum.EFCore)
- [Persisting a Smart Enum with Entity Framework Core](https://blog.nimblepros.com/blogs/persisting-a-smart-enum-with-entity-framework-core/)
- [How to Improve Enums With the SmartEnum Library](https://code-maze.com/csharp-improve-enums-with-the-smartenum-library/)

### Specification Pattern
- [Ardalis.Specification GitHub Repository](https://github.com/ardalis/Specification)
- [Ardalis.Specification Documentation](http://specification.ardalis.com/)
- [Ardalis.Specification 9.3.1 on NuGet](https://www.nuget.org/packages/Ardalis.Specification)
- [Ardalis.Specification.EntityFrameworkCore 9.3.1 on NuGet](https://www.nuget.org/packages/Ardalis.Specification.EntityFrameworkCore)
- [Getting Started With Specifications](https://blog.nimblepros.com/blogs/getting-started-with-specifications/)
- [Ardalis Specification v9 Released](https://ardalis.com/ardalis-specification-v9-release/)

### StronglyTypedId Source Generators
- [Meziantou.Framework.StronglyTypedId 2.3.11 on NuGet](https://www.nuget.org/packages/Meziantou.Framework.StronglyTypedId)
- [Meziantou.Framework GitHub Repository](https://github.com/meziantou/Meziantou.Framework)
- [Strongly-typed Ids using C# Source Generators](https://www.meziantou.net/strongly-typed-ids-with-csharp-source-generators.htm)
- [StronglyTypedId (andrewlock) 1.0.0-beta08 on NuGet](https://www.nuget.org/packages/StronglyTypedId/1.0.0-beta08)
- [Rebuilding StronglyTypedId as a source generator](https://andrewlock.net/rebuilding-stongly-typed-id-as-a-source-generator-1-0-0-beta-release/)

### Entity Framework Core Integration
- [EF Core 10.0.3 on NuGet](https://www.nuget.org/packages/microsoft.entityframeworkcore)
- [What's New in EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [EF Core Interceptors Documentation](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [EF Core SaveChangesInterceptor for Auditing Entities](https://mehmetozkaya.medium.com/ef-core-interceptors-savechangesinterceptor-for-auditing-entities-in-net-8-microservices-6923190a03b9)
- [How to Implement Audit Logs with EF Core Interceptors](https://oneuptime.com/blog/post/2026-01-25-audit-logs-ef-core-interceptors/view)
- [Handling Concurrency Conflicts in EF Core](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [Concurrency Management in Entity Framework Core](https://www.learnentityframeworkcore.com/concurrency)

### DDD Entity Patterns
- [Entity Base Class - Enterprise Craftsmanship](https://enterprisecraftsmanship.com/posts/entity-base-class/)
- [Building an Enterprise Data Access Layer: Automated Auditing](https://byteaether.github.io/2025/building-an-enterprise-data-access-layer-automated-auditing/)
- [Creating Domain-Driven Design entity classes with Entity Framework Core](https://www.thereformedprogrammer.net/creating-domain-driven-design-entity-classes-with-entity-framework-core/)
- [Clean DDD lessons: audit metadata for domain entities](https://medium.com/unil-ci-software-engineering/clean-ddd-lessons-audit-metadata-for-domain-entities-5935a5c6db5b)

### Avoiding Library Bloat
- [Dependency Management in .NET Libraries: A Guide for Library Authors](https://medium.com/@osama.abusitta/dependency-management-in-net-libraries-a-guide-for-library-authors-part-2-37a76a8559af)
- [Dependency Hell: The Hidden Costs of Dependency Bloat in Software Development](https://oneuptime.com/blog/post/2025-09-02-the-hidden-costs-of-dependency-bloat-in-software-development/view)
- [Designing a DDD-oriented microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)

---
*Stack research for: DDD Building Blocks Improvements for .NET 10*
*Researched: 2026-02-14*
*Next: Review FEATURES.md, ARCHITECTURE.md, PITFALLS.md for implementation guidance*
