# Phase 19: Specification Pattern - Complex Query Logic - Research

**Researched:** 2026-02-24
**Domain:** Ardalis.Specification + EF Core query pattern
**Confidence:** HIGH

## Summary

Phase 19 extracts complex inline query logic from MediatR query handlers into reusable, unit-testable Specification objects using the Ardalis.Specification library. The codebase currently has all filtering, sorting, and pagination logic embedded directly inside `GetProductsQueryHandler`, `GetAllOrdersQueryHandler`, and `GetOrdersByBuyerQueryHandler`. These handlers construct `IQueryable` chains inline, which is not reusable and is only testable through integration tests.

Ardalis.Specification is already aligned with the project stack: the project uses Ardalis.SmartEnum and Ardalis.GuardClauses, making Ardalis.Specification a natural fit. The library provides `Specification<T>` base class with a fluent `Query` builder supporting `Where`, `Include`, `OrderBy`, `Skip`, `Take`, and crucially `WithSpecification` extension method for `IQueryable` — enabling direct use against a `DbSet` without a repository wrapper (matching the project's explicit "Generic Repository is out of scope" REQUIREMENTS.md constraint).

The key architectural decision for this phase: Ardalis.Specification is used in "DbContext mode" (via `WithSpecification` extension on `DbSet<T>`) rather than the full repository pattern. This gives specification testability and reuse without requiring a generic repository, keeping EF Core DbContext as the unit of work per project conventions.

**Primary recommendation:** Add `Ardalis.Specification` (v9.3.1) to `BuildingBlocks.Common` and `Ardalis.Specification.EntityFrameworkCore` (v9.3.1) to `MicroCommerce.ApiService`. Create specification base infrastructure in `BuildingBlocks.Common`, then extract five specifications for Catalog and Ordering query handlers.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| QUERY-01 | Specification pattern base classes (Ardalis.Specification) integrated with EF Core DbContext | `Ardalis.Specification` v9.3.1 core + `Ardalis.Specification.EntityFrameworkCore` v9.3.1 for `WithSpecification`. Install in BuildingBlocks.Common and ApiService respectively. |
| QUERY-02 | Catalog specifications (PublishedProductsSpec, ProductsByCategorySpec, ProductSearchSpec) extracted from handlers into specifications | Extracted from `GetProductsQueryHandler` inline `Where` + `OrderBy` + `Skip/Take` chains. Use Ardalis fluent `Query.Where().OrderBy().Skip().Take()` builder pattern. |
| QUERY-03 | Ordering specifications (ActiveOrdersSpec, OrdersByBuyerSpec) extracted from handlers into specifications | Extracted from `GetAllOrdersQueryHandler` (active orders, status filter) and `GetOrdersByBuyerQueryHandler` (buyer filter + status). Both handlers share identical projection — spec handles filter, handler keeps projection. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Ardalis.Specification | 9.3.1 | Base `Specification<T>` class, `ISpecification<T>` interface, fluent `Query` builder | The authoritative .NET specification library; already used in project via SmartEnum/GuardClauses family |
| Ardalis.Specification.EntityFrameworkCore | 9.3.1 | `WithSpecification(spec)` extension on `IQueryable<T>`, `SpecificationEvaluator` | Required for EF Core integration; without this, spec can't be applied to `DbSet` |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| xunit (already installed) | 2.9.3 | Unit test framework | Test specs via `IsSatisfiedBy()` without EF or database |
| FluentAssertions (already installed) | 7.0.0 | Test assertions | Already in test project — use for spec unit tests |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Ardalis.Specification | Hand-rolled Specification<T> | REQUIREMENTS.md says "Don't Hand-Roll" for generic solutions; Ardalis handles expression tree composition, pagination, includes, AsNoTracking — all complex |
| Ardalis.Specification | LinqKit PredicateBuilder | LinqKit is predicate-only, no ordering/paging/include support; Ardalis is a full query specification |

**Installation:**
```bash
# In BuildingBlocks.Common (spec base classes — no EF dependency)
dotnet add src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj package Ardalis.Specification --version 9.3.1

# In MicroCommerce.ApiService (EF Core integration — WithSpecification extension)
dotnet add src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj package Ardalis.Specification.EntityFrameworkCore --version 9.3.1
```

## Architecture Patterns

### Recommended Project Structure

```
src/
BuildingBlocks/BuildingBlocks.Common/
  Specifications/              # (NEW) Base spec infrastructure
    ISpecification.cs          # Re-export or marker (Ardalis provides ISpecification<T>)

MicroCommerce.ApiService/
  Features/
    Catalog/
      Application/
        Specifications/        # (NEW) Catalog-specific specs
          PublishedProductsSpec.cs
          ProductsByCategorySpec.cs
          ProductSearchSpec.cs
        Queries/
          GetProducts/
            GetProductsQueryHandler.cs    # Updated to use specs
    Ordering/
      Application/
        Specifications/        # (NEW) Ordering-specific specs
          ActiveOrdersSpec.cs
          OrdersByBuyerSpec.cs
        Queries/
          GetAllOrders/
            GetAllOrdersQueryHandler.cs   # Updated to use specs
          GetOrdersByBuyer/
            GetOrdersByBuyerQueryHandler.cs  # Updated to use specs

MicroCommerce.ApiService.Tests/
  Unit/
    Catalog/
      Specifications/          # (NEW) Unit tests — no DB required
        PublishedProductsSpecTests.cs
        ProductsByCategorySpecTests.cs
        ProductSearchSpecTests.cs
    Ordering/
      Specifications/          # (NEW) Unit tests — no DB required
        ActiveOrdersSpecTests.cs
        OrdersByBuyerSpecTests.cs
```

### Pattern 1: Define Specification with Ardalis Fluent Builder

**What:** Extend `Specification<T>` from `Ardalis.Specification`, use `Query.Where(...)` in constructor.
**When to use:** Any complex filter, ordering, or pagination that appears in multiple handlers or is complex enough to test.

```csharp
// Source: https://github.com/ardalis/specification/blob/main/docs/usage/use-specification-dbcontext.md
using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class PublishedProductsSpec : Specification<Product>
{
    public PublishedProductsSpec()
    {
        Query.Where(p => p.Status == ProductStatus.Published)
             .OrderByDescending(p => p.CreatedAt);
    }
}

public sealed class ProductsByCategorySpec : Specification<Product>
{
    public ProductsByCategorySpec(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId);
    }
}

public sealed class ProductSearchSpec : Specification<Product>
{
    public ProductSearchSpec(string searchTerm)
    {
        string lower = searchTerm.ToLower();
        Query.Where(p =>
            p.Name.Value.ToLower().Contains(lower) ||
            p.Description.ToLower().Contains(lower) ||
            (p.Sku != null && p.Sku.ToLower().Contains(lower)));
    }
}
```

### Pattern 2: Apply Specification to DbSet (No Repository)

**What:** Use `WithSpecification` extension method directly on `DbSet<T>`, keeping EF Core DbContext as the unit of work.
**When to use:** This project explicitly excludes Generic Repository — use DbContext direct mode.

```csharp
// Source: https://github.com/ardalis/specification/blob/main/docs/usage/use-specification-dbcontext.md
using Ardalis.Specification.EntityFrameworkCore;

// In handler:
var spec = new PublishedProductsSpec();
var products = await _context.Products
    .AsNoTracking()
    .WithSpecification(spec)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync(cancellationToken);
```

### Pattern 3: Specification Composition via And/Or

**What:** Ardalis v9 supports `And`, `Or`, `Not` composition returning new `Specification<T>` instances.
**When to use:** Success criterion 5 — demonstrate composition for combined filters (e.g., category + status + search).

```csharp
// From GetProductsQueryHandler: build composed spec based on query parameters
Specification<Product> spec = new AllProductsSpec(); // base: no filter

if (request.CategoryId.HasValue)
    spec = spec.And(new ProductsByCategorySpec(CategoryId.From(request.CategoryId.Value)));

if (!string.IsNullOrWhiteSpace(request.Status) &&
    ProductStatus.TryFromName(request.Status, ignoreCase: true, out ProductStatus? status))
    spec = spec.And(new ProductByStatusSpec(status!));

if (!string.IsNullOrWhiteSpace(request.Search))
    spec = spec.And(new ProductSearchSpec(request.Search));

var totalCount = await _context.Products
    .WithSpecification(spec)
    .CountAsync(cancellationToken);
```

### Pattern 4: Unit-Testable Specifications via IsSatisfiedBy

**What:** `Specification<T>` implements `IsSatisfiedBy(T entity)` which evaluates the expression tree in-memory — no database required.
**When to use:** Success criterion 4 — unit test specifications in isolation.

```csharp
// Source: Ardalis.Specification README
[Fact]
public void PublishedProductsSpec_ExcludesDraftProducts()
{
    // Arrange
    Product draftProduct = Product.Create(
        ProductName.Create("Test"), "Desc", Money.Create(10m), CategoryId.New());
    // product starts as Draft
    PublishedProductsSpec spec = new();

    // Act
    bool result = spec.IsSatisfiedBy(draftProduct);

    // Assert
    result.Should().BeFalse();
}

[Fact]
public void ProductsByCategorySpec_MatchesCategoryId()
{
    // Arrange
    CategoryId targetId = CategoryId.New();
    Product product = Product.Create(
        ProductName.Create("P"), "D", Money.Create(10m), targetId);
    ProductsByCategorySpec spec = new(targetId);

    // Act + Assert
    spec.IsSatisfiedBy(product).Should().BeTrue();
}
```

### Pattern 5: Ordering with Ardalis Specification

**What:** Use `Query.OrderBy()` / `Query.OrderByDescending()` / `ThenBy()` inside the specification.
**When to use:** When sort logic belongs to the specification (e.g., published products always sort by CreatedAt desc).

```csharp
// Source: https://github.com/ardalis/specification/blob/main/docs/features/take.md
public sealed class ActiveOrdersSpec : Specification<Order>
{
    private static readonly OrderStatus[] TerminalStatuses =
        [OrderStatus.Failed, OrderStatus.Cancelled];

    public ActiveOrdersSpec(string? statusFilter = null)
    {
        Query.Where(o => !TerminalStatuses.Contains(o.Status));

        if (statusFilter is not null &&
            OrderStatus.TryFromName(statusFilter, ignoreCase: true, out OrderStatus? status))
        {
            Query.Where(o => o.Status == status);
        }

        Query.OrderByDescending(o => o.CreatedAt);
    }
}

public sealed class OrdersByBuyerSpec : Specification<Order>
{
    public OrdersByBuyerSpec(Guid buyerId, string? statusFilter = null)
    {
        Query.Where(o => o.BuyerId == buyerId);

        if (statusFilter is not null &&
            OrderStatus.TryFromName(statusFilter, ignoreCase: true, out OrderStatus? status))
        {
            Query.Where(o => o.Status == status);
        }

        Query.OrderByDescending(o => o.CreatedAt);
    }
}
```

### Anti-Patterns to Avoid

- **Putting projection logic in specifications:** Ardalis `Specification<T>` is for filtering/ordering/includes, not for `.Select()` projections. Keep the `Select(o => new OrderSummaryDto(...))` in the handler. Handlers call `WithSpecification(spec)` then chain their own `.Select()`.
- **Generic Repository:** REQUIREMENTS.md explicitly excludes this. Do not add `IRepository<T>` or `RepositoryBase<T>`. Use `_context.DbSet.WithSpecification(spec)` directly.
- **Putting specs in the Domain layer:** These are query/infrastructure concerns, not domain invariants. Place in `Application/Specifications/` per vertical slice structure.
- **Overusing specifications:** Simple single-predicate queries (`Where(p => p.Id == id)`) don't need a spec. Only extract when: complex logic, reused across handlers, or explicitly required by success criteria.
- **Skip/Take inside specifications for reuse:** Pagination inside a spec makes it non-reusable for count queries (count must not have Skip/Take). Either put pagination in the handler or use separate spec for count vs. list queries. See pitfall below.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Specification base class | Custom `ISpecification<T>` with expression trees | `Ardalis.Specification` v9.3.1 | Expression tree composition (And/Or/Not) is non-trivial; Ardalis handles parameter replacement correctly |
| EF Core query application | Custom `SpecificationEvaluator` | `Ardalis.Specification.EntityFrameworkCore` `.WithSpecification()` | Handles Where, OrderBy, ThenBy, Include, Skip, Take, AsNoTracking, IgnoreQueryFilters in correct order |
| Predicate composition | `LinqKit.PredicateBuilder` | `Ardalis.Specification` composition | Ardalis supports full spec composition with ordering/includes; PredicateBuilder is predicate-only |

**Key insight:** Expression tree manipulation for `And/Or` composition requires correct parameter rebinding that is subtle to get right. Ardalis handles this internally — hand-rolling leads to `System.InvalidOperationException: variable 'x' of type referenced from scope` errors.

## Common Pitfalls

### Pitfall 1: Pagination Inside Specification Breaks Count Queries

**What goes wrong:** A spec with `.Skip(skip).Take(take)` cannot be reused for `CountAsync` — it returns wrong count.
**Why it happens:** EF Core translates Skip/Take into SQL OFFSET/FETCH; COUNT(*) with OFFSET/FETCH gives subset count not total.
**How to avoid:** Keep Skip/Take in the handler, not the specification. Apply spec for filtering, then chain Skip/Take:
```csharp
var totalCount = await _context.Products.WithSpecification(spec).CountAsync(ct);
var items = await _context.Products
    .WithSpecification(spec)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(...)
    .ToListAsync(ct);
```
**Warning signs:** CountAsync returns same number as PageSize instead of total records.

### Pitfall 2: Multiple Where Calls on Same Specification

**What goes wrong:** Calling `Query.Where(...)` twice in one specification constructor is valid — Ardalis ANDs them. However, conditional `Where` calls inside the constructor need care with nullable checks.
**Why it happens:** Developers assume a second `Query.Where(...)` replaces the first — it doesn't, it adds AND.
**How to avoid:** This is correct behavior for AND logic. For OR, use `spec.Or(otherSpec)` composition.

### Pitfall 3: IsSatisfiedBy Does Not Evaluate Navigation Properties

**What goes wrong:** Unit tests using `IsSatisfiedBy` with navigation properties that are null (not loaded) fail or return wrong results.
**Why it happens:** `IsSatisfiedBy` compiles the expression and evaluates in-memory; navigation properties are null on in-memory entities.
**How to avoid:** For specs that test navigation properties (e.g., `o.Items.Count > 0`), use in-memory setup that populates those collections, or test via integration tests with a real DB.

### Pitfall 4: SmartEnum Comparison in Expression Trees

**What goes wrong:** Ardalis `IsSatisfiedBy` compiles expression tree in-memory; SmartEnum equality works fine because SmartEnum implements `==` via `Equals`. But if any spec uses `Contains()` on a list of SmartEnum instances, EF Core translation may fail.
**Why it happens:** `List<OrderStatus>.Contains(o.Status)` with SmartEnum requires EF Core to translate to SQL `IN (...)` using the string converter. This works with `Ardalis.SmartEnum` + EF Core string converter from `SmartEnumStringConverter` already set up in `BaseDbContext`.
**How to avoid:** Keep the existing `ExcludedStatuses` pattern from `GetOrderDashboardQueryHandler` — it already works. The `BaseDbContext` `ConfigureConventions` registers `SmartEnumStringConverter<OrderStatus>` and `SmartEnumStringConverter<ProductStatus>` for all properties, ensuring proper SQL translation.

### Pitfall 5: Vogen Value Object Comparison in Specs

**What goes wrong:** `p.CategoryId == categoryId` in spec expression works for EF Core translation (Vogen handles equality), but `IsSatisfiedBy` in-memory may behave differently if the Vogen struct doesn't implement `==` correctly.
**Why it happens:** Vogen generates `readonly record struct` with value equality, so `==` and `Equals` work correctly. This is not a real issue but worth verifying for first spec.
**How to avoid:** Test `IsSatisfiedBy` explicitly in unit tests with matching and non-matching CategoryId values to confirm.

## Code Examples

Verified patterns from official sources:

### Specification With Filtering Only (Handler Does Projection)
```csharp
// Source: https://github.com/ardalis/specification/blob/main/docs/usage/use-specification-dbcontext.md
// Spec handles filter + order; handler handles count, pagination, projection

// In handler:
PublishedProductsSpec spec = new();
int totalCount = await _context.Products
    .AsNoTracking()
    .WithSpecification(spec)
    .CountAsync(cancellationToken);

List<ProductDto> items = await _context.Products
    .AsNoTracking()
    .WithSpecification(spec)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .Join(_context.Categories.AsNoTracking(), ...)
    .ToListAsync(cancellationToken);
```

### Composed Specification for GetProducts
```csharp
// Demonstrates Success Criterion 5: And/Or composition
// Source: Ardalis.Specification composition API (verified via Context7)
Specification<Product> spec = new ProductsBaseSpec(); // OrderByDescending(p => p.CreatedAt)

if (request.CategoryId.HasValue)
    spec = spec.And(new ProductsByCategorySpec(CategoryId.From(request.CategoryId.Value)));

if (productStatus is not null)
    spec = spec.And(new ProductByStatusSpec(productStatus));

if (!string.IsNullOrWhiteSpace(request.Search))
    spec = spec.And(new ProductSearchSpec(request.Search));
```

### Unit Test for Specification
```csharp
// No DB, no EF — pure in-memory evaluation
[Fact]
public void ActiveOrdersSpec_ExcludesFailedOrders()
{
    // Arrange - create in-memory Order with Failed status
    // (requires internal constructor or test factory)
    ActiveOrdersSpec spec = new();

    // Act + Assert — uses IsSatisfiedBy (compiles expression in-memory)
    spec.IsSatisfiedBy(failedOrder).Should().BeFalse();
    spec.IsSatisfiedBy(submittedOrder).Should().BeTrue();
}
```

### Install Both Packages
```bash
dotnet add src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj \
    package Ardalis.Specification --version 9.3.1

dotnet add src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj \
    package Ardalis.Specification.EntityFrameworkCore --version 9.3.1
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Hand-rolled `ISpecification<T>` with `ToExpression()` | `Ardalis.Specification` fluent builder with `Query.Where().OrderBy()` | Ardalis v5+ | No more raw expression trees; fluent API is readable and composable |
| Repository pattern required for specs | `WithSpecification` extension on `DbSet<T>` directly | Ardalis v6+ | Specs work without Generic Repository — matches this project's explicit constraint |
| Specs only for predicates | Full query specs (filter + sort + includes + pagination) | Ardalis v7+ | Single spec object captures entire query intent |

**No deprecated features relevant to this phase.**

## Open Questions

1. **Where does `Specification<T>` base live — BuildingBlocks or ApiService?**
   - What we know: `Ardalis.Specification` is the base package (no EF dependency). `Ardalis.Specification.EntityFrameworkCore` requires EF Core (already in BuildingBlocks.Common).
   - What's unclear: Should `Ardalis.Specification` be in BuildingBlocks.Common (so domain layer could use it) or just in ApiService?
   - Recommendation: Add `Ardalis.Specification` to `BuildingBlocks.Common` (it has no EF Core dependency and BuildingBlocks.Common already has EF Core). Add `Ardalis.Specification.EntityFrameworkCore` to `MicroCommerce.ApiService` only. Concrete spec classes live in `Application/Specifications/` per feature module.

2. **Unit-testability: Order entity construction for unit tests**
   - What we know: `Order` has `private Order(OrderId id)` constructor called only by `Order.Create(...)`. Unit testing `ActiveOrdersSpec.IsSatisfiedBy(order)` requires an `Order` instance with specific `Status`.
   - What's unclear: `Order.Status` is set to `OrderStatus.Submitted` in `Create()`, but testing `Failed` state requires calling `MarkAsFailed()` then checking spec.
   - Recommendation: For `ActiveOrdersSpec` unit tests, create an order via `Order.Create(...)` then call `MarkAsFailed("reason")` or `Confirm()` to reach different states. This is already demonstrated in existing `OrderTests.cs`. The `Failed` and `Delivered` statuses require saga-driven transitions — may need internal test helpers.

3. **GetProductsQueryHandler Join with Categories**
   - What we know: `GetProductsQueryHandler` uses `.Join(_context.Categories, ...)` to get category name for DTO. This projection happens after spec filtering.
   - What's unclear: Specs return `IQueryable<Product>` — the Join stays in the handler. Planner should confirm specs don't attempt to include Category (that's a Join, not Include, due to DTO shape).
   - Recommendation: Confirm in plan that spec applies only to `Products` table; the `.Join()` for CategoryName projection remains in handler code unchanged.

## Validation Architecture

> Nyquist validation is not enabled (workflow.nyquist_validation not present in config.json). Skip section.

## Sources

### Primary (HIGH confidence)
- `/ardalis/specification` (Context7) — `IsSatisfiedBy`, `WithSpecification`, `Query.Where/OrderBy/Skip/Take`, composition API, pagination patterns
- NuGet API (`api.nuget.org`) — Ardalis.Specification 9.3.1, Ardalis.Specification.EntityFrameworkCore 9.3.1 (latest as of 2026-02-24)
- Official docs: `https://github.com/ardalis/specification/blob/main/docs/usage/use-specification-dbcontext.md` — DbContext mode without repository

### Secondary (MEDIUM confidence)
- Codebase analysis: `GetProductsQueryHandler`, `GetAllOrdersQueryHandler`, `GetOrdersByBuyerQueryHandler` — actual inline query logic to be extracted
- Codebase analysis: `BaseDbContext.ConfigureConventions` — SmartEnum converter registration confirmed working for EF Core translation
- Codebase analysis: Existing `ProductTests.cs`, `OrderTests.cs` unit test patterns — confirms test structure for new spec unit tests

### Tertiary (LOW confidence)
- None. All claims verified via Context7 or codebase inspection.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — Ardalis.Specification version confirmed via NuGet API; API verified via Context7
- Architecture: HIGH — DbContext mode (no repository) confirmed in official docs; aligns with project REQUIREMENTS.md constraint
- Pitfalls: HIGH — Pagination/count issue and expression tree constraints verified via Context7 examples; SmartEnum/Vogen integration confirmed via existing BaseDbContext code

**Research date:** 2026-02-24
**Valid until:** 2026-03-24 (Ardalis.Specification stable library, 30-day validity)
