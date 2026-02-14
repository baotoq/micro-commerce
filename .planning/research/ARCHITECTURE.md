# Architecture Research: DDD Building Blocks Integration

**Domain:** DDD Building Blocks Integration with Existing .NET 10 Modular Monolith
**Researched:** 2026-02-14
**Confidence:** HIGH

## Executive Summary

MicroCommerce has an existing DDD foundation with BaseAggregateRoot, StronglyTypedId, DomainEvent, and DomainEventInterceptor. The new building blocks (Entity base, audit interfaces, concurrency, Result type, Enumeration class, Specification pattern, source generators) integrate by extending this foundation, not replacing it. Integration happens at three key points: EF Core configurations, MediatR pipeline behaviors, and domain entity inheritance hierarchy.

**Key Integration Principle:** Additive, not disruptive. Existing aggregates continue working unchanged while new building blocks provide opt-in capabilities.

## Existing Architecture Snapshot

### Current Building Blocks

```
BuildingBlocks.Common/
├── BaseAggregateRoot<TId>        # Aggregate root base with domain events
├── IAggregateRoot                # Marker interface
├── StronglyTypedId<T>            # Base record for typed IDs
├── ValueObject                   # OBSOLETE class-based value objects
├── Events/
│   ├── DomainEvent               # Event base with EventId
│   ├── IDomainEvent              # Event marker interface
│   └── EventId                   # StronglyTypedId<Guid> for events
```

### Current Domain Model Pattern

```csharp
// Aggregate Root
public sealed class Order : BaseAggregateRoot<OrderId>
{
    [Timestamp]
    public uint Version { get; private set; }  // PostgreSQL xmin concurrency

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order(OrderId id) : base(id) { }

    public static Order Create(...)
    {
        var order = new Order(OrderId.New()) { ... };
        order.AddDomainEvent(new OrderSubmittedDomainEvent(...));
        return order;
    }
}

// Child Entity (No base class currently)
public sealed class OrderItem
{
    public OrderItemId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    private OrderItem() { }
    internal static OrderItem Create(...) => new OrderItem { ... };
}

// StronglyTypedId
public sealed record OrderId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
}

// Value Object (readonly record struct pattern)
public readonly record struct ProductName
{
    public string Value { get; init; }
    private ProductName(string value) => Value = value;
    public static ProductName Create(string value) { ... }
}
```

### Current EF Core Integration

**DbContext per Feature Module:**
```csharp
public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");
        modelBuilder.ApplyConfigurationsFromAssembly(...);
    }
}
```

**Value Converters for StronglyTypedId:**
```csharp
builder.Property(o => o.Id)
    .HasConversion(
        id => id.Value,
        value => OrderId.From(value));
```

**Complex Properties for Value Objects:**
```csharp
builder.ComplexProperty(p => p.Price, priceBuilder =>
{
    priceBuilder.Property(m => m.Amount)
        .HasColumnName("Price")
        .HasPrecision(18, 2);
});
```

**PostgreSQL xmin for Concurrency:**
```csharp
builder.Property(o => o.Version).IsRowVersion();  // Maps to xmin
```

### Current MediatR Pipeline

**Single Pipeline Behavior:**
```csharp
// ValidationBehavior<TRequest, TResponse>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(...)
    {
        if (!_validators.Any()) return await next();

        var failures = await ValidateAsync(request);
        if (failures.Any()) throw new ValidationException(failures);

        return await next();
    }
}
```

**Handler Pattern (Exception-based):**
```csharp
public record SubmitOrderCommand(...) : IRequest<Guid>;

public class SubmitOrderCommandHandler : IRequestHandler<SubmitOrderCommand, Guid>
{
    public async Task<Guid> Handle(SubmitOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(...);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        return order.Id.Value;  // Direct Guid return
    }
}
```

**Exception Handling:**
- GlobalExceptionHandler maps ValidationException, NotFoundException, ConflictException to ProblemDetails
- Exceptions propagate up through MediatR pipeline

### Current Domain Event Publishing

**DomainEventInterceptor:**
```csharp
public class DomainEventInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(...)
    {
        // After SaveChanges completes:
        var aggregates = context.ChangeTracker.Entries<IAggregateRoot>();
        var events = aggregates.SelectMany(a => a.DomainEvents);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var @event in events)
            await _publishEndpoint.Publish(@event, @event.GetType(), ct);
    }
}
```

## New Building Blocks Integration

### 1. Entity Base Class with Audit

**New Component:**
```csharp
// BuildingBlocks.Common/Entity.cs
public abstract class Entity<TId> : IEntity<TId>, IAuditable
{
    public TId Id { get; protected init; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    protected Entity(TId id) => Id = id;
}

// Interfaces
public interface IEntity<TId> { TId Id { get; } }
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}
```

**Integration Point: Aggregate Hierarchy**

**EXISTING (unchanged):**
```csharp
public sealed class Order : BaseAggregateRoot<OrderId>  // Still works
{
    // No audit properties — teams choose when to add
}
```

**NEW (opt-in audit):**
```csharp
public abstract class AuditableAggregateRoot<TId> : BaseAggregateRoot<TId>, IAuditable
{
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    protected AuditableAggregateRoot(TId id) : base(id) { }
}

// Opt-in usage
public sealed class Product : AuditableAggregateRoot<ProductId>
{
    private Product(ProductId id) : base(id) { }
}
```

**Integration Point: Child Entities**

**BEFORE (no base class):**
```csharp
public sealed class OrderItem
{
    public OrderItemId Id { get; private set; }
    private OrderItem() { }
}
```

**AFTER (opt-in Entity base):**
```csharp
public sealed class OrderItem : Entity<OrderItemId>
{
    private OrderItem(OrderItemId id) : base(id) { }
}
```

**Integration Point: EF Core AuditInterceptor**

**New Component:**
```csharp
// ApiService/Common/Persistence/AuditInterceptor.cs
public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, ...)
    {
        var entries = eventData.Context?.ChangeTracker
            .Entries<IAuditable>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = DateTimeOffset.UtcNow;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = DateTimeOffset.UtcNow;
        }

        return base.SavingChanges(eventData, result);
    }
}
```

**DbContext Registration (per-module):**
```csharp
services.AddDbContext<OrderingDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
        .AddInterceptors(
            sp.GetRequiredService<DomainEventInterceptor>(),  // Existing
            sp.GetRequiredService<AuditInterceptor>());       // NEW
});
```

**Data Flow:**
```
SaveChangesAsync()
    ↓
AuditInterceptor.SavingChanges()          // Sets CreatedAt/UpdatedAt
    ↓
[Database write]
    ↓
DomainEventInterceptor.SavedChangesAsync() // Publishes domain events
```

### 2. Concurrency Handling

**Current Approach:** Direct PostgreSQL xmin mapping
```csharp
[Timestamp]
public uint Version { get; private set; }
```

**New Building Block (optional pattern):**
```csharp
// BuildingBlocks.Common/IConcurrencyToken.cs
public interface IConcurrencyToken { uint Version { get; } }

// Opt-in usage in aggregates
public sealed class Order : BaseAggregateRoot<OrderId>, IConcurrencyToken
{
    [Timestamp]
    public uint Version { get; private set; }
}
```

**Integration Point: EF Core Configuration Convention**

**New Convention (applied globally):**
```csharp
// ApiService/Common/Persistence/ConcurrencyTokenConvention.cs
public class ConcurrencyTokenConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, ...)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (typeof(IConcurrencyToken).IsAssignableFrom(entityType.ClrType))
            {
                entityType.FindProperty(nameof(IConcurrencyToken.Version))
                    ?.SetIsRowVersion(true);
            }
        }
    }
}

// DbContext
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Conventions.Add(_ => new ConcurrencyTokenConvention());
}
```

**Benefit:** Removes repetitive `builder.Property(o => o.Version).IsRowVersion()` in every configuration.

**Conflict Handling (unchanged):**
```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    throw new ConflictException("Order was modified by another transaction.");
}
```

### 3. Result Type Pattern

**New Component:**
```csharp
// BuildingBlocks.Common/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result.");

    private Result(TValue value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) => _value = default;

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(Error error) => new(error);
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
```

**Integration Point: MediatR Commands/Queries**

**BEFORE (exception-based):**
```csharp
public record SubmitOrderCommand(...) : IRequest<Guid>;

public class Handler : IRequestHandler<SubmitOrderCommand, Guid>
{
    public async Task<Guid> Handle(...)
    {
        if (cart.Items.Count == 0)
            throw new ValidationException("Cart is empty");  // Exception path

        var order = Order.Create(...);
        await _context.SaveChangesAsync();
        return order.Id.Value;
    }
}
```

**AFTER (Result-based, opt-in):**
```csharp
public record SubmitOrderCommand(...) : IRequest<Result<Guid>>;

public class Handler : IRequestHandler<SubmitOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(...)
    {
        if (cart.Items.Count == 0)
            return Result<Guid>.Failure(new Error("Cart.Empty", "Cart is empty"));

        var order = Order.Create(...);
        await _context.SaveChangesAsync();
        return Result<Guid>.Success(order.Id.Value);
    }
}
```

**Integration Point: Endpoint Mapping**

**BEFORE:**
```csharp
app.MapPost("/orders", async (SubmitOrderCommand cmd, ISender sender) =>
{
    try
    {
        var orderId = await sender.Send(cmd);
        return Results.Ok(orderId);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Errors);
    }
});
```

**AFTER (with Result extension):**
```csharp
// ApiService/Common/Extensions/ResultExtensions.cs
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

// Endpoint
app.MapPost("/orders", async (SubmitOrderCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.ToHttpResult();
});
```

**Integration Point: Pipeline Behavior (optional)**

**New ValidationBehavior for Result:**
```csharp
public class ResultValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, ...)
    {
        if (!_validators.Any()) return await next();

        var failures = await ValidateAsync(request);
        if (failures.Any())
        {
            var error = new Error("Validation.Failed", string.Join(", ", failures.Select(f => f.ErrorMessage)));
            return (TResponse)(object)Result.Failure(error);  // No exception thrown
        }

        return await next();
    }
}
```

**Coexistence Strategy:**
- **Exception-based:** `IRequest<T>` handlers continue using ValidationBehavior (throws exception)
- **Result-based:** `IRequest<Result<T>>` handlers use ResultValidationBehavior (returns failure)
- Register both behaviors, MediatR resolves based on TResponse constraint

### 4. Enumeration Class (Smart Enums)

**Current State:** Using primitive enums
```csharp
public enum OrderStatus
{
    Submitted,
    StockReserved,
    Paid,
    // ...
}
```

**New Building Block:**
```csharp
// BuildingBlocks.Common/Enumeration.cs
public abstract class Enumeration<TEnum> : IComparable<TEnum>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> _allValues = new(() =>
        typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(TEnum))
            .Select(f => (TEnum)f.GetValue(null)!)
            .ToDictionary(e => e.Value));

    public string Name { get; }
    public int Value { get; }

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static IReadOnlyCollection<TEnum> GetAll() => _allValues.Value.Values.ToList();
    public static TEnum FromValue(int value) => _allValues.Value[value];
    public static TEnum? FromName(string name) => GetAll().FirstOrDefault(e => e.Name == name);

    public override string ToString() => Name;
    public int CompareTo(TEnum? other) => Value.CompareTo(other?.Value);
}
```

**Integration Point: Domain Model**

**Migration Path (backward compatible):**
```csharp
// Step 1: Create enumeration class (enum stays for now)
public sealed class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Submitted = new(0, nameof(Submitted));
    public static readonly OrderStatus StockReserved = new(1, nameof(StockReserved));
    public static readonly OrderStatus Paid = new(2, nameof(Paid));
    public static readonly OrderStatus Confirmed = new(3, nameof(Confirmed));

    private OrderStatus(int value, string name) : base(value, name) { }

    // Business logic methods
    public bool CanTransitionTo(OrderStatus targetStatus) => (this, targetStatus) switch
    {
        (var s, var t) when s == Submitted && t == StockReserved => true,
        (var s, var t) when s == StockReserved && t == Paid => true,
        (var s, var t) when s == Paid && t == Confirmed => true,
        _ => false
    };

    public bool IsTerminal() => this == Delivered || this == Failed || this == Cancelled;
}
```

**Integration Point: EF Core Value Converter**

**New Converter:**
```csharp
// ApiService/Common/Persistence/EnumerationValueConverter.cs
public class EnumerationValueConverter<TEnum> : ValueConverter<TEnum, int>
    where TEnum : Enumeration<TEnum>
{
    public EnumerationValueConverter()
        : base(
            enumeration => enumeration.Value,
            value => Enumeration<TEnum>.FromValue(value))
    {
    }
}

// Configuration
builder.Property(o => o.Status)
    .HasConversion(new EnumerationValueConverter<OrderStatus>())
    .HasMaxLength(32)
    .IsRequired();
```

**Integration Point: API Serialization**

**JSON Converter:**
```csharp
public class EnumerationJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : Enumeration<TEnum>
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, ...)
    {
        var value = reader.GetString();
        return Enumeration<TEnum>.FromName(value)
            ?? throw new JsonException($"Unknown {typeof(TEnum).Name}: {value}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, ...)
    {
        writer.WriteStringValue(value.Name);
    }
}

// Register globally
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new EnumerationJsonConverter<OrderStatus>());
});
```

### 5. Specification Pattern

**New Component:**
```csharp
// BuildingBlocks.Common/Specification/ISpecification.cs
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

// Base specification
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    // ... other properties

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;
    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; IsPagingEnabled = true; }
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
}
```

**Integration Point: Query Handlers**

**BEFORE (ad-hoc LINQ):**
```csharp
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(...)
    {
        var query = _context.Products
            .Where(p => p.Status == ProductStatus.Published);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == new CategoryId(request.CategoryId.Value));

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(p => p.Name.Value.Contains(request.Search));

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(...))
            .ToListAsync();
    }
}
```

**AFTER (with Specification):**
```csharp
// Specification
public class PublishedProductsSpec : Specification<Product>
{
    public PublishedProductsSpec(Guid? categoryId, string? search, int page, int pageSize)
    {
        AddCriteria(p => p.Status == ProductStatus.Published);

        if (categoryId.HasValue)
            AddCriteria(p => p.CategoryId == new CategoryId(categoryId.Value));

        if (!string.IsNullOrEmpty(search))
            AddCriteria(p => p.Name.Value.Contains(search));

        ApplyOrderByDescending(p => p.CreatedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}

// Specification Evaluator
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec)
        where T : class
    {
        var query = inputQuery;

        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);

        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);

        return query;
    }
}

// Handler
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(...)
    {
        var spec = new PublishedProductsSpec(request.CategoryId, request.Search, request.Page, request.PageSize);

        return await SpecificationEvaluator
            .GetQuery(_context.Products, spec)
            .Select(p => new ProductDto(...))
            .ToListAsync();
    }
}
```

**Benefits:**
- Specifications are testable in isolation (unit tests without database)
- Reusable across queries
- Complex business rules encapsulated
- Easier to understand than scattered LINQ

**Integration Point: Repository Pattern (optional)**

MicroCommerce currently uses DbContext directly in handlers (no repository layer). If adding repositories later:

```csharp
public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync(ISpecification<T> spec);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);
}

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;

    public async Task<List<T>> ListAsync(ISpecification<T> spec)
    {
        return await SpecificationEvaluator.GetQuery(_context.Set<T>(), spec).ToListAsync();
    }
}
```

### 6. Source Generators for StronglyTypedId

**Current Pattern (manual):**
```csharp
public sealed record OrderId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
}

// EF Core configuration (manual in every entity configuration)
builder.Property(o => o.Id)
    .HasConversion(
        id => id.Value,
        value => OrderId.From(value));
```

**New Building Block: Source Generator Integration**

**Option 1: StronglyTypedId Library**
```csharp
// Add NuGet: StronglyTypedId
// BuildingBlocks.Common/StronglyTypedIdConfig.cs

[assembly: StronglyTypedIdDefaults(
    backingType: StronglyTypedIdBackingType.Guid,
    converters: StronglyTypedIdConverter.EfCoreValueConverter | StronglyTypedIdConverter.SystemTextJson)]

// Usage
[StronglyTypedId]
public partial struct OrderId { }

// Generated code includes:
// - OrderId(Guid value) constructor
// - Guid Value property
// - static OrderId New()
// - EF Core ValueConverter
// - System.Text.Json JsonConverter
```

**Option 2: Custom Source Generator (more control)**

**Integration Point: EF Core Convention**

**Problem:** Even with source-generated ValueConverters, still need to apply them:
```csharp
builder.Property(o => o.Id).HasConversion(new OrderIdValueConverter());  // Repetitive
```

**Solution: Global ValueConverter Convention**
```csharp
// ApiService/Common/Persistence/StronglyTypedIdConvention.cs
public class StronglyTypedIdConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, ...)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (IsStronglyTypedId(property.ClrType))
                {
                    var converterType = typeof(StronglyTypedIdValueConverter<>).MakeGenericType(property.ClrType);
                    var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                    property.SetValueConverter(converter);
                }
            }
        }
    }

    private static bool IsStronglyTypedId(Type type)
    {
        return type.BaseType is { IsGenericType: true }
            && type.BaseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>);
    }
}

// DbContext
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
}
```

**Result:** No more manual `.HasConversion()` calls. Convention auto-applies converters to all StronglyTypedId properties.

**Integration Point: JSON Serialization**

**Problem:** StronglyTypedId serializes as object by default:
```json
{ "orderId": { "value": "123e4567-e89b-12d3-a456-426614174000" } }
```

**Solution: Custom JsonConverter**
```csharp
// BuildingBlocks.Common/StronglyTypedIdJsonConverter.cs
public class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.BaseType is { IsGenericType: true }
            && typeToConvert.BaseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.BaseType!.GetGenericArguments()[0];
        var converterType = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class StronglyTypedIdJsonConverter<TId, TValue> : JsonConverter<TId>
    where TId : StronglyTypedId<TValue>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
        return (TId)Activator.CreateInstance(typeToConvert, value)!;
    }

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

// Registration
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
});
```

**Result:**
```json
{ "orderId": "123e4567-e89b-12d3-a456-426614174000" }
```

## Recommended Project Structure

### BuildingBlocks.Common (New Components)

```
BuildingBlocks.Common/
├── BaseAggregateRoot.cs              # EXISTING (unchanged)
├── AuditableAggregateRoot.cs         # NEW (inherits BaseAggregateRoot)
├── Entity.cs                         # NEW (base for child entities)
├── IAuditable.cs                     # NEW (audit interface)
├── IConcurrencyToken.cs              # NEW (concurrency marker)
├── StronglyTypedId.cs                # EXISTING (unchanged)
├── ValueObject.cs                    # EXISTING (obsolete, keep for migration)
├── Result/
│   ├── Result.cs                     # NEW (success/failure result)
│   ├── Result{T}.cs                  # NEW (generic result)
│   └── Error.cs                      # NEW (error record)
├── Enumeration/
│   └── Enumeration{TEnum}.cs         # NEW (smart enum base)
├── Specification/
│   ├── ISpecification{T}.cs          # NEW (specification interface)
│   ├── Specification{T}.cs           # NEW (specification base)
│   └── SpecificationEvaluator.cs     # NEW (EF Core query builder)
├── Events/                           # EXISTING (unchanged)
│   ├── DomainEvent.cs
│   ├── IDomainEvent.cs
│   └── EventId.cs
└── Converters/
    ├── StronglyTypedIdJsonConverterFactory.cs    # NEW
    └── EnumerationJsonConverter{TEnum}.cs        # NEW
```

### ApiService/Common (New Infrastructure)

```
ApiService/Common/
├── Behaviors/
│   ├── ValidationBehavior.cs                     # EXISTING (exception-based)
│   └── ResultValidationBehavior.cs               # NEW (Result-based)
├── Persistence/
│   ├── DomainEventInterceptor.cs                 # EXISTING (unchanged)
│   ├── AuditInterceptor.cs                       # NEW
│   ├── Conventions/
│   │   ├── StronglyTypedIdConvention.cs          # NEW (auto value converters)
│   │   └── ConcurrencyTokenConvention.cs         # NEW (auto concurrency)
│   └── Converters/
│       └── EnumerationValueConverter{TEnum}.cs   # NEW
├── Extensions/
│   └── ResultExtensions.cs                       # NEW (Result → IResult)
└── Exceptions/                                   # EXISTING (unchanged)
    ├── ValidationException.cs
    ├── NotFoundException.cs
    └── GlobalExceptionHandler.cs
```

### Feature Module Integration

```
Features/Ordering/
├── Domain/
│   ├── Entities/
│   │   ├── Order.cs                 # BEFORE: BaseAggregateRoot<OrderId>
│   │   │                            # AFTER:  AuditableAggregateRoot<OrderId> (opt-in)
│   │   └── OrderItem.cs             # BEFORE: No base class
│   │                                # AFTER:  Entity<OrderItemId> (opt-in)
│   ├── ValueObjects/
│   │   ├── OrderStatus.cs           # BEFORE: enum
│   │   │                            # AFTER:  Enumeration<OrderStatus> (migration)
│   │   └── OrderId.cs               # BEFORE: Manual record
│   │                                # AFTER:  [StronglyTypedId] partial struct (opt-in)
│   └── Specifications/              # NEW
│       └── ActiveOrdersSpec.cs      # Reusable query logic
├── Application/
│   ├── Commands/
│   │   └── SubmitOrder/
│   │       ├── SubmitOrderCommand.cs    # BEFORE: IRequest<Guid>
│   │       │                            # AFTER:  IRequest<Result<Guid>> (opt-in)
│   │       └── SubmitOrderHandler.cs    # Returns Result<Guid>
│   └── Queries/
│       └── GetOrders/
│           ├── GetOrdersQuery.cs        # Uses specifications
│           └── GetOrdersHandler.cs      # SpecificationEvaluator integration
└── Infrastructure/
    ├── OrderingDbContext.cs         # Adds AuditInterceptor, conventions
    └── Configurations/
        └── OrderConfiguration.cs    # BEFORE: Manual .HasConversion()
                                     # AFTER:  Convention auto-applies (cleaner)
```

## Data Flow Changes

### Before: Exception-Based Flow

```
HTTP Request
    ↓
Endpoint → MediatR.Send(Command)
    ↓
ValidationBehavior
    ├─ Validation fails → throw ValidationException
    └─ Validation passes → next()
         ↓
    CommandHandler
         ├─ Business rule fails → throw InvalidOperationException
         └─ Success → return TResponse
              ↓
         DbContext.SaveChangesAsync()
              ↓
         DomainEventInterceptor (publishes events)
              ↓
    Endpoint returns Results.Ok(response)

[On Exception]
    GlobalExceptionHandler catches → ProblemDetails → HTTP 4xx
```

### After: Result-Based Flow (Opt-In)

```
HTTP Request
    ↓
Endpoint → MediatR.Send(Command)
    ↓
ResultValidationBehavior
    ├─ Validation fails → return Result.Failure(error)  [NO EXCEPTION]
    └─ Validation passes → next()
         ↓
    CommandHandler
         ├─ Business rule fails → return Result.Failure(error)  [NO EXCEPTION]
         └─ Success → return Result.Success(value)
              ↓
         DbContext.SaveChangesAsync()
              ↓
         AuditInterceptor (sets CreatedAt/UpdatedAt)  [NEW]
              ↓
         [Database write]
              ↓
         DomainEventInterceptor (publishes events)
              ↓
    Endpoint → result.ToHttpResult() → HTTP 200/400

[Exceptions only for infrastructure failures]
```

### Audit Timestamps Flow

```
Entity implements IAuditable
    ↓
SaveChangesAsync() called
    ↓
AuditInterceptor.SavingChanges()
    ├─ EntityState.Added → Set CreatedAt = UtcNow
    └─ EntityState.Modified → Set UpdatedAt = UtcNow
         ↓
[Database write with timestamps]
```

### StronglyTypedId Conversion Flow

```
Domain Layer: Order { OrderId Id }
    ↓
EF Core Write: StronglyTypedIdConvention
    ├─ Detects StronglyTypedId<Guid>
    └─ Auto-applies ValueConverter → Guid column
         ↓
Database: orders.id (uuid column)
         ↓
EF Core Read: ValueConverter
    ├─ Guid from DB → OrderId.From(guid)
    └─ Returns OrderId
         ↓
JSON Serialization: StronglyTypedIdJsonConverter
    └─ OrderId → "uuid-string" (not {"value": "uuid-string"})
```

## Anti-Patterns to Avoid

### Anti-Pattern 1: Forcing Result on All Handlers

**What teams do:** Mandate `IRequest<Result<T>>` for all commands/queries immediately.

**Why it's wrong:**
- Breaking change for existing handlers
- Not all operations benefit from Result pattern (simple CRUD doesn't need it)
- Increases boilerplate where exceptions are fine

**Do this instead:**
- Use Result for complex business operations with expected failures (checkout, payment)
- Keep exceptions for unexpected errors (database down, network timeout)
- Migrate incrementally, starting with high-value scenarios

### Anti-Pattern 2: Audit All The Things

**What teams do:** Make every entity inherit `AuditableAggregateRoot` or `Entity<TId>`.

**Why it's wrong:**
- Child entities owned by aggregates don't need audit trails (OrderItem audit = Order audit)
- Creates database bloat (CreatedAt/UpdatedAt on every table)
- Performance overhead in interceptors

**Do this instead:**
- Audit aggregate roots only (Order, Product, not OrderItem)
- Use event sourcing for true audit trails if compliance requires every change
- Shadow properties for audit if domain shouldn't know about timestamps

### Anti-Pattern 3: Specification Everywhere

**What teams do:** Wrap every LINQ query in a Specification class.

**Why it's wrong:**
- Overkill for simple queries (`_context.Products.FindAsync(id)`)
- Adds indirection without value
- Harder to debug than inline LINQ

**Do this instead:**
- Use specifications for complex, reusable business queries (filtering, sorting, paging)
- Keep simple queries inline in handlers
- Introduce specifications when query logic repeats across handlers

### Anti-Pattern 4: Premature Source Generator Adoption

**What teams do:** Replace all StronglyTypedId records with `[StronglyTypedId] partial struct` immediately.

**Why it's wrong:**
- Source generators add build complexity (diagnostics, IntelliSense lag)
- Existing record pattern works fine
- Migration effort doesn't justify benefits for small codebases

**Do this instead:**
- Start with manual records
- Add source generator when you have 20+ StronglyTypedId types
- Use for new modules, migrate old ones gradually

### Anti-Pattern 5: Enumeration Without Behavior

**What teams do:** Convert all enums to Enumeration classes "because DDD."

**Why it's wrong:**
- If enum has no behavior (just labels), primitive enum is simpler
- Enumeration adds overhead (reflection, dictionary lookup)
- JSON serialization complexity

**Do this instead:**
- Keep primitive enums for simple states (ProductStatus.Draft/Published)
- Use Enumeration when adding behavior (OrderStatus.CanTransitionTo(), IsTerminal())
- Migrate when enum logic scatters across handlers

## Integration Checklist

### Phase 1: Foundation (Non-Breaking)

- [ ] Add `IAuditable`, `IConcurrencyToken` interfaces to BuildingBlocks.Common
- [ ] Add `Entity<TId>` base class to BuildingBlocks.Common
- [ ] Add `AuditableAggregateRoot<TId>` to BuildingBlocks.Common
- [ ] Implement `AuditInterceptor` in ApiService/Common/Persistence
- [ ] Register `AuditInterceptor` in all DbContexts (opt-out via interface check)
- [ ] Existing aggregates unchanged, continue working

### Phase 2: Result Pattern (Opt-In)

- [ ] Add `Result`, `Result<T>`, `Error` to BuildingBlocks.Common
- [ ] Add `ResultExtensions.ToHttpResult()` to ApiService/Common/Extensions
- [ ] Implement `ResultValidationBehavior<TRequest, TResponse>` where TResponse : Result
- [ ] Register ResultValidationBehavior in DI (coexists with ValidationBehavior)
- [ ] Migrate 1-2 complex command handlers to Result pattern (pilot)
- [ ] Evaluate developer experience, adjust before broader rollout

### Phase 3: Conventions (DRY Improvements)

- [ ] Implement `StronglyTypedIdConvention` for auto value converters
- [ ] Implement `ConcurrencyTokenConvention` for auto row versioning
- [ ] Add conventions to DbContext.ConfigureConventions in each module
- [ ] Remove manual `.HasConversion()` calls from entity configurations
- [ ] Test migrations to ensure no schema changes

### Phase 4: Enumeration (Selective Migration)

- [ ] Add `Enumeration<TEnum>` base class to BuildingBlocks.Common
- [ ] Implement `EnumerationValueConverter<TEnum>` for EF Core
- [ ] Implement `EnumerationJsonConverter<TEnum>` for JSON
- [ ] Identify enums with behavior (OrderStatus, PaymentStatus)
- [ ] Migrate enums to Enumeration classes one at a time
- [ ] Keep primitive enums for simple labels (ProductStatus can stay enum)

### Phase 5: Specifications (Targeted Adoption)

- [ ] Add `ISpecification<T>`, `Specification<T>`, `SpecificationEvaluator` to BuildingBlocks.Common
- [ ] Identify complex queries with repeated logic (product filtering, order search)
- [ ] Create specification classes for reusable queries
- [ ] Update handlers to use SpecificationEvaluator
- [ ] Leave simple queries inline (no specifications)

### Phase 6: Source Generators (Optional)

- [ ] Evaluate StronglyTypedId NuGet package vs. custom generator
- [ ] Add source generator to BuildingBlocks.Common project
- [ ] Configure assembly-level defaults for converters
- [ ] Add `StronglyTypedIdJsonConverterFactory` to JSON options
- [ ] Pilot with new feature module (e.g., Payments)
- [ ] Gradual migration of existing StronglyTypedId records

## Build Order Recommendations

**Safe Migration Path (Minimal Risk):**

1. **Phase 1 (Foundation)** — Audit interfaces and Entity base classes are additive. Deploy with confidence.
2. **Phase 3 (Conventions)** — DRY improvements with no behavior changes. Low risk.
3. **Phase 2 (Result Pattern)** — Pilot with 1-2 handlers, evaluate, then scale if beneficial.
4. **Phase 4 (Enumeration)** — Migrate enums with behavior first (OrderStatus), leave simple enums alone.
5. **Phase 5 (Specifications)** — Adopt for complex queries only, leave simple queries unchanged.
6. **Phase 6 (Source Generators)** — Optional optimization after 20+ StronglyTypedId types.

**Critical Dependencies:**

- **AuditInterceptor** depends on `IAuditable` interface
- **ResultValidationBehavior** depends on `Result` type
- **Conventions** depend on marker interfaces (`IConcurrencyToken`, `StronglyTypedId<T>`)
- **Enumeration EF Core support** depends on `EnumerationValueConverter<TEnum>`

**No Breaking Changes:** All new building blocks are opt-in. Existing code continues working unchanged.

## Sources

**DDD Building Blocks:**
- [Seedwork (reusable base classes and interfaces for your domain model) - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/seedwork-domain-model-base-classes-interfaces)
- [Entity Base Class - Enterprise Craftsmanship](https://enterprisecraftsmanship.com/posts/entity-base-class/)
- [Clean DDD lessons: audit metadata for domain entities - Medium](https://medium.com/unil-ci-software-engineering/clean-ddd-lessons-audit-metadata-for-domain-entities-5935a5c6db5b)

**EF Core Audit:**
- [How to Implement Audit Logs with EF Core Interceptors](https://oneuptime.com/blog/post/2026-01-25-audit-logs-ef-core-interceptors/view)
- [EF Core Interceptors: SaveChangesInterceptor for Auditing Entities - Medium](https://mehmetozkaya.medium.com/ef-core-interceptors-savechangesinterceptor-for-auditing-entities-in-net-8-microservices-6923190a03b9)
- [Tracking Every Change: Using SaveChanges Interception for EF Core Auditing](https://www.woodruff.dev/tracking-every-change-using-savechanges-interception-for-ef-core-auditing/)
- [How to Build Custom EF Core Conventions](https://oneuptime.com/blog/post/2026-01-30-build-custom-ef-core-conventions/view)

**Result Pattern:**
- [Improving Error Handling with the Result Pattern in MediatR](https://goatreview.com/improving-error-handling-result-pattern-mediatr/)
- [GitHub - altmann/FluentResults](https://github.com/altmann/FluentResults)

**MediatR Pipeline:**
- [CQRS Validation with MediatR Pipeline and FluentValidation](https://www.milanjovanovic.tech/blog/cqrs-validation-with-mediatr-pipeline-and-fluentvalidation)
- [Validation without Exceptions using a MediatR Pipeline Behavior - Medium](https://medium.com/the-cloud-builders-guild/validation-without-exceptions-using-a-mediatr-pipeline-behavior-278f124836dc)
- [Rethinking MediatR Validation: Moving from Pipeline to Domain Objects](https://goatreview.com/rethinking-mediatr-pipeline-validation-pattern/)

**Enumeration Classes:**
- [Using Enumeration classes instead of enum types - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types)
- [Smart Enums: Beyond Traditional Enumerations in .NET - Thinktecture AG](https://www.thinktecture.com/en/net/smart-enums-beyond-traditional-enumerations-in-dotnet/)

**Specification Pattern:**
- [How to use Specifications with the Repository Pattern](https://specification.ardalis.com/usage/use-specification-repository-pattern.html)
- [Specification Pattern in EF Core: Flexible Data Access Without Repositories](https://antondevtips.com/blog/specification-pattern-in-ef-core-flexible-data-access-without-repositories)
- [GitHub - ardalis/Specification](https://github.com/ardalis/Specification)

**Strongly Typed IDs:**
- [GitHub - andrewlock/StronglyTypedId](https://github.com/andrewlock/StronglyTypedId)
- [A Better Way to Handle Entity Identification in .NET with Strongly Typed IDs](https://antondevtips.com/blog/a-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)
- [Rebuilding StronglyTypedId as a source generator](https://andrewlock.net/rebuilding-stongly-typed-id-as-a-source-generator-1-0-0-beta-release/)
- [Using strongly-typed entity IDs with EF Core](https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-3/)
- [Value Conversions - EF Core - Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

---
*Architecture research for: DDD Building Blocks Integration*
*Researched: 2026-02-14*
