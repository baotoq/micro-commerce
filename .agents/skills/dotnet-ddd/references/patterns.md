# Supporting Patterns

Common patterns that complement DDD in .NET.

## Result Pattern

Use the Result pattern when a domain operation can fail for **expected business reasons** (not exceptional errors). Avoids using exceptions for flow control.

### Basic Result

```csharp
public sealed class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
}

public sealed class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;

    private Result(T value) => Value = value;
    private Result(Error error) => Error = error;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}
```

### Error Types

```csharp
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

// Define errors as static members per domain concept
public static class OrderErrors
{
    public static readonly Error EmptyOrder = new("Order.Empty", "Cannot submit an empty order");
    public static readonly Error AlreadyShipped = new("Order.AlreadyShipped", "Order has already been shipped");
    public static readonly Error NotFound = new("Order.NotFound", "Order not found");
}
```

### Using Result in Aggregates

```csharp
public sealed class Order : Entity<OrderId>
{
    public Result Submit()
    {
        if (_lines.Count == 0)
            return Result.Failure(OrderErrors.EmptyOrder);

        if (Status != OrderStatus.Draft)
            return Result.Failure(new Error("Order.InvalidStatus", $"Cannot submit order in {Status} status"));

        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmittedEvent(Id, Total));
        return Result.Success();
    }
}
```

### Using Result in Application Services

```csharp
public sealed class SubmitOrderHandler(IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result> HandleAsync(SubmitOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var result = order.Submit();
        if (result.IsFailure)
            return result;

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

## Strongly-Typed IDs

Prevent mixing up IDs of different entity types. `OrderId` and `CustomerId` are different types even though both wrap `Guid`.

### Using `readonly record struct`

```csharp
public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public static CustomerId From(Guid value) => new(value);
}

public readonly record struct ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
}
```

### EF Core Value Converter

```csharp
public sealed class OrderIdConverter : ValueConverter<OrderId, Guid>
{
    public OrderIdConverter() : base(
        id => id.Value,
        value => OrderId.From(value))
    { }
}

// In DbContext OnModelCreating
modelBuilder.Entity<Order>()
    .Property(o => o.Id)
    .HasConversion<OrderIdConverter>();
```

### JSON Converter

```csharp
public sealed class OrderIdJsonConverter : JsonConverter<OrderId>
{
    public override OrderId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        OrderId.From(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, OrderId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
```

## Specification Pattern

Encapsulate query logic as reusable, composable objects. Keeps query logic in the domain layer.

### Specification Interface

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}

public abstract class Specification<T> : ISpecification<T>
{
    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);
    public abstract Expression<Func<T, bool>> ToExpression();

    public Specification<T> And(Specification<T> other) =>
        new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other) =>
        new OrSpecification<T>(this, other);

    public Specification<T> Not() =>
        new NotSpecification<T>(this);
}
```

### Composite Specifications

```csharp
internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal sealed class NotSpecification<T>(Specification<T> spec) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = spec.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.Not(Expression.Invoke(expr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
```

### Concrete Specifications

```csharp
public sealed class ActiveOrdersSpec : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Status != OrderStatus.Cancelled
              && order.Status != OrderStatus.Completed;
}

public sealed class OrdersByCustomerSpec(CustomerId customerId) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.CustomerId == customerId;
}

public sealed class HighValueOrdersSpec(decimal threshold) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Lines.Sum(l => l.UnitPrice.Amount * l.Quantity) > threshold;
}

// Compose specifications
var spec = new ActiveOrdersSpec()
    .And(new OrdersByCustomerSpec(customerId))
    .And(new HighValueOrdersSpec(1000));
```

### Repository with Specification

```csharp
public interface IRepository<T, TId>
    where T : Entity<TId>
    where TId : notnull
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
}
```

## Guard Clauses

Validate preconditions at the boundary of domain methods:

```csharp
public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{paramName} cannot be null or empty")
            : value;

    public static T NotNull<T>(T? value, string paramName) where T : class =>
        value ?? throw new DomainException($"{paramName} cannot be null");

    public static int Positive(int value, string paramName) =>
        value > 0 ? value : throw new DomainException($"{paramName} must be positive");

    public static decimal NotNegative(decimal value, string paramName) =>
        value >= 0 ? value : throw new DomainException($"{paramName} cannot be negative");
}

// Usage
public sealed class Product : Entity<ProductId>
{
    public string Name { get; private set; }
    public Money Price { get; private set; }

    public static Product Create(string name, Money price) =>
        new(ProductId.New())
        {
            Name = Guard.NotNullOrWhiteSpace(name, nameof(name)),
            Price = Guard.NotNull(price, nameof(price))
        };
}
```

## Unit of Work

Coordinates saving changes across multiple repositories in a single transaction:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// DbContext already implements Unit of Work — just expose the interface:
public sealed class AppDbContext : DbContext, IUnitOfWork
{
    // SaveChangesAsync is already implemented by DbContext
}
```

Register in DI:

```csharp
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
```

## Enumeration Classes

Replace primitive enums with rich domain types when behavior is needed:

```csharp
public abstract class Enumeration<TEnum>(int id, string name)
    : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    public int Id { get; } = id;
    public string Name { get; } = name;

    public bool Equals(Enumeration<TEnum>? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Enumeration<TEnum> other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;

    public static IReadOnlyList<TEnum> GetAll() =>
        typeof(TEnum).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(TEnum))
            .Select(f => (TEnum)f.GetValue(null)!)
            .ToList();

    public static TEnum FromId(int id) =>
        GetAll().FirstOrDefault(e => e.Id == id)
        ?? throw new DomainException($"Invalid {typeof(TEnum).Name} id: {id}");
}

// Usage
public sealed class OrderStatus(int id, string name) : Enumeration<OrderStatus>(id, name)
{
    public static readonly OrderStatus Draft = new(1, nameof(Draft));
    public static readonly OrderStatus Submitted = new(2, nameof(Submitted));
    public static readonly OrderStatus Shipped = new(3, nameof(Shipped));
    public static readonly OrderStatus Completed = new(4, nameof(Completed));
    public static readonly OrderStatus Cancelled = new(5, nameof(Cancelled));

    public bool CanTransitionTo(OrderStatus next) => (this, next) switch
    {
        _ when this == Draft => next == Submitted || next == Cancelled,
        _ when this == Submitted => next == Shipped || next == Cancelled,
        _ when this == Shipped => next == Completed,
        _ => false
    };
}
```
