---
name: dotnet-ddd
description: "Domain-Driven Design patterns for .NET. Covers aggregates, entities, value objects, domain events, repositories, specifications, and bounded contexts. Use when modeling complex business domains."
metadata:
  version: "1.0.0"
  domain: architecture
  triggers: DDD, domain-driven design, aggregate, value object, domain event, bounded context, entity, repository pattern, specification pattern, domain model
  role: specialist
  scope: implementation
  output-format: code
  related-skills: csharp-developer, efcore-patterns, microservices-architect
---

# Domain-Driven Design in .NET

## When to Use This Skill

Use this skill when:
- Modeling complex business domains with rich behavior
- Designing aggregates, entities, and value objects
- Implementing domain events for decoupled communication
- Defining bounded contexts and context maps
- Building repositories that respect aggregate boundaries
- Applying the specification pattern for query encapsulation
- Enforcing invariants and business rules in the domain layer

## Core Principles

1. **Ubiquitous Language** - Code mirrors business terminology exactly
2. **Aggregates as Consistency Boundaries** - Each aggregate enforces its own invariants
3. **Value Objects Over Primitives** - Wrap domain concepts in value objects
4. **Rich Domain Models** - Business logic lives in the domain, not in services
5. **Side-Effect-Free Functions** - Value object operations return new instances
6. **Explicit Over Implicit** - Make domain concepts explicit types, not strings/ints
7. **Persistence Ignorance** - Domain model has no dependency on infrastructure

---

## Building Blocks

### Entity Base Class

Entities have identity and a lifecycle. Two entities are equal if they have the same ID.

```csharp
public abstract class Entity
{
    public Guid Id { get; protected init; }

    protected Entity() => Id = Guid.CreateVersion7();

    public override bool Equals(object? obj) =>
        obj is Entity other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) =>
        !Equals(left, right);
}
```

### Aggregate Root

Aggregates are the transactional consistency boundary. All changes go through the root.

```csharp
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Domain Events

Events represent something that happened in the domain. They are past-tense, immutable facts.

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

// Concrete event
public sealed record OrderPlaced(Guid OrderId, decimal Total) : DomainEvent;
public sealed record OrderCancelled(Guid OrderId, string Reason) : DomainEvent;
```

### Value Objects

Value objects have no identity. They are defined by their attributes and are immutable.

```csharp
public abstract record ValueObject;

// Simple value objects - use records for structural equality
public sealed record Money(decimal Amount, string Currency) : ValueObject
{
    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot add {Currency} and {other.Currency}");

        return this with { Amount = Amount + other.Amount };
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract {Currency} and {other.Currency}");

        return this with { Amount = Amount - other.Amount };
    }

    public Money Multiply(decimal factor) =>
        this with { Amount = Amount * factor };
}

public sealed record Address(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : ValueObject;

public sealed record DateRange(DateOnly Start, DateOnly End) : ValueObject
{
    public int Days => End.DayNumber - Start.DayNumber;

    public bool Contains(DateOnly date) =>
        date >= Start && date <= End;

    public bool Overlaps(DateRange other) =>
        Start <= other.End && End >= other.Start;
}
```

### Strongly-Typed IDs

Prevent primitive obsession by wrapping IDs in dedicated types. This prevents accidentally passing a `CustomerId` where an `OrderId` is expected.

```csharp
// Option 1: Record struct (lightweight, stack-allocated)
public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

// Usage - compiler prevents mixing IDs
public class Order
{
    public OrderId Id { get; private init; }
    public CustomerId CustomerId { get; private init; }
}

// EF Core configuration for strongly-typed IDs
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(o => o.CustomerId)
            .HasConversion(id => id.Value, value => new CustomerId(value));
    }
}
```

---

## Aggregate Design

### Rules for Aggregates

1. **Protect invariants inside the aggregate** - All state changes go through methods that validate rules
2. **Reference other aggregates by ID only** - Never hold a direct object reference to another aggregate
3. **Keep aggregates small** - Favor smaller aggregates; use domain events for cross-aggregate coordination
4. **One transaction per aggregate** - Don't modify multiple aggregates in a single transaction
5. **Design for eventual consistency** - Cross-aggregate consistency is achieved through domain events

### Example: Order Aggregate

```csharp
public class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    public CustomerId CustomerId { get; private init; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private Order() { } // EF Core

    public static Order Create(CustomerId customerId, string currency)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            Total = Money.Zero(currency),
            CreatedAt = DateTimeOffset.UtcNow
        };

        order.RaiseDomainEvent(new OrderCreated(order.Id, customerId));
        return order;
    }

    public void AddLine(ProductId productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify a non-draft order.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
        {
            existingLine.IncreaseQuantity(quantity);
        }
        else
        {
            _lines.Add(OrderLine.Create(productId, productName, unitPrice, quantity));
        }

        RecalculateTotal();
    }

    public void RemoveLine(ProductId productId)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify a non-draft order.");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId)
            ?? throw new DomainException($"Product {productId} not found in order.");

        _lines.Remove(line);
        RecalculateTotal();
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be placed.");

        if (_lines.Count == 0)
            throw new DomainException("Cannot place an empty order.");

        Status = OrderStatus.Placed;
        RaiseDomainEvent(new OrderPlaced(Id, Total.Amount));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException("Cannot cancel a completed order.");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelled(Id, reason));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Placed)
            throw new DomainException("Only placed orders can be completed.");

        Status = OrderStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    private void RecalculateTotal()
    {
        Total = _lines.Aggregate(
            Money.Zero(Total.Currency),
            (sum, line) => sum.Add(line.Subtotal));
    }
}

public class OrderLine : Entity
{
    public ProductId ProductId { get; private init; }
    public string ProductName { get; private init; } = null!;
    public Money UnitPrice { get; private init; } = null!;
    public int Quantity { get; private set; }
    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private OrderLine() { } // EF Core

    internal static OrderLine Create(
        ProductId productId, string productName, Money unitPrice, int quantity)
    {
        return new OrderLine
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive.");

        Quantity += amount;
    }
}

public enum OrderStatus
{
    Draft,
    Placed,
    Completed,
    Cancelled
}
```

### Key Takeaways

- **Private setters** - State is only changed through methods that enforce rules
- **Factory methods** (`Create`) - Ensure valid initial state and raise creation events
- **Internal methods** on child entities - Only the aggregate root can trigger changes
- **No public constructors** for entities inside the aggregate - Prevent creating orphaned children
- **Encapsulated collections** - Expose `IReadOnlyList`, mutate through aggregate methods

---

## Domain Exceptions

Use a dedicated exception type for business rule violations. These are distinct from infrastructure exceptions.

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Optional: specific exception types for different rule categories
public class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}
```

### Alternative: Result Pattern

For expected failures, prefer a `Result<T>` over exceptions.

```csharp
public sealed record Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T value) => Value = value;
    private Result(string error) => Error = error;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}

// Usage in domain
public Result<Order> PlaceOrder()
{
    if (Status != OrderStatus.Draft)
        return Result<Order>.Failure("Only draft orders can be placed.");

    if (_lines.Count == 0)
        return Result<Order>.Failure("Cannot place an empty order.");

    Status = OrderStatus.Placed;
    RaiseDomainEvent(new OrderPlaced(Id, Total.Amount));
    return Result<Order>.Success(this);
}
```

---

## Repository Pattern

Repositories provide collection-like access to aggregates. One repository per aggregate root.

```csharp
// Domain layer - interface
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        CustomerId customerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    void Remove(Order order);
}

// Infrastructure layer - implementation
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Orders
            .Include(o => o.Lines)
            .AsTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        CustomerId customerId, CancellationToken ct = default) =>
        await _db.Orders
            .Include(o => o.Lines)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await _db.Orders.AddAsync(order, ct);

    public void Update(Order order) => _db.Orders.Update(order);

    public void Remove(Order order) => _db.Orders.Remove(order);
}
```

### Unit of Work

Coordinate saves across multiple repositories in a single transaction.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// DbContext already implements Unit of Work
public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    // Dispatch domain events on save
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync();
        return result;
    }

    private async Task DispatchDomainEventsAsync()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}
```

---

## Specification Pattern

Encapsulate query logic in reusable, composable specification objects.

```csharp
public abstract class Specification<T> where T : class
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity) =>
        ToExpression().Compile()(entity);

    public Specification<T> And(Specification<T> other) =>
        new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other) =>
        new OrSpecification<T>(this, other);

    public Specification<T> Not() =>
        new NotSpecification<T>(this);
}

internal sealed class AndSpecification<T>(
    Specification<T> left,
    Specification<T> right) : Specification<T>
    where T : class
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

internal sealed class OrSpecification<T>(
    Specification<T> left,
    Specification<T> right) : Specification<T>
    where T : class
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

internal sealed class NotSpecification<T>(
    Specification<T> spec) : Specification<T>
    where T : class
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = spec.ToExpression();
        var param = Expression.Parameter(typeof(T));

        var body = Expression.Not(Expression.Invoke(expr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

// Concrete specifications
public sealed class ActiveOrdersSpec : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Status == OrderStatus.Placed;
}

public sealed class OrdersByCustomerSpec(CustomerId customerId) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.CustomerId == customerId;
}

public sealed class HighValueOrdersSpec(decimal threshold) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Total.Amount >= threshold;
}

// Usage
var spec = new ActiveOrdersSpec()
    .And(new OrdersByCustomerSpec(customerId))
    .And(new HighValueOrdersSpec(1000));

var orders = await _db.Orders
    .Where(spec.ToExpression())
    .ToListAsync();
```

---

## Domain Services

For business logic that doesn't naturally belong to a single entity or value object.

```csharp
// Domain service - when logic spans multiple aggregates
public class PricingService
{
    public Money CalculateDiscount(Order order, Customer customer)
    {
        var discount = customer.Tier switch
        {
            CustomerTier.Gold => 0.10m,
            CustomerTier.Silver => 0.05m,
            _ => 0m
        };

        return order.Total.Multiply(discount);
    }
}

// Domain service - when logic requires external data but is still domain logic
public interface IExchangeRateService
{
    Task<decimal> GetRateAsync(string from, string to, CancellationToken ct = default);
}

public class MoneyConverter(IExchangeRateService exchangeRates)
{
    public async Task<Money> ConvertAsync(
        Money money, string targetCurrency, CancellationToken ct = default)
    {
        if (money.Currency == targetCurrency)
            return money;

        var rate = await exchangeRates.GetRateAsync(
            money.Currency, targetCurrency, ct);

        return new Money(money.Amount * rate, targetCurrency);
    }
}
```

---

## Bounded Contexts

### Project Structure

Organize code by bounded context, not by technical layer.

```
src/
├── Ordering/                          # Bounded context
│   ├── Ordering.Domain/               # Entities, value objects, events, interfaces
│   │   ├── Aggregates/
│   │   │   └── Order/
│   │   │       ├── Order.cs
│   │   │       ├── OrderLine.cs
│   │   │       └── OrderStatus.cs
│   │   ├── ValueObjects/
│   │   │   └── Money.cs
│   │   ├── Events/
│   │   │   ├── OrderPlaced.cs
│   │   │   └── OrderCancelled.cs
│   │   ├── Specifications/
│   │   │   └── ActiveOrdersSpec.cs
│   │   ├── Services/
│   │   │   └── PricingService.cs
│   │   └── Interfaces/
│   │       └── IOrderRepository.cs
│   ├── Ordering.Application/          # Use cases, commands, queries
│   │   ├── Commands/
│   │   │   └── PlaceOrder/
│   │   │       ├── PlaceOrderCommand.cs
│   │   │       └── PlaceOrderHandler.cs
│   │   └── Queries/
│   │       └── GetOrderById/
│   │           ├── GetOrderByIdQuery.cs
│   │           └── GetOrderByIdHandler.cs
│   └── Ordering.Infrastructure/       # EF Core, external services
│       ├── Persistence/
│       │   ├── OrderingDbContext.cs
│       │   ├── OrderConfiguration.cs
│       │   └── OrderRepository.cs
│       └── DependencyInjection.cs
├── Catalog/                           # Another bounded context
│   ├── Catalog.Domain/
│   ├── Catalog.Application/
│   └── Catalog.Infrastructure/
└── SharedKernel/                      # Shared across contexts
    ├── Entity.cs
    ├── AggregateRoot.cs
    ├── ValueObject.cs
    └── IDomainEvent.cs
```

### Anti-Corruption Layer

When integrating with other bounded contexts or external systems, use an ACL to translate between models.

```csharp
// Ordering context needs customer data from Customer context
// Don't reference Customer.Domain directly

// Define what Ordering needs
public record OrderingCustomerInfo(
    CustomerId Id,
    string Name,
    CustomerTier Tier);

// Anti-corruption layer translates external model
public interface ICustomerInfoService
{
    Task<OrderingCustomerInfo?> GetCustomerInfoAsync(
        CustomerId id, CancellationToken ct = default);
}

// Implementation calls the Customer context API
public class CustomerInfoService(HttpClient httpClient) : ICustomerInfoService
{
    public async Task<OrderingCustomerInfo?> GetCustomerInfoAsync(
        CustomerId id, CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<CustomerDto>(
            $"/api/customers/{id}", ct);

        if (response is null) return null;

        return new OrderingCustomerInfo(
            new CustomerId(response.Id),
            response.FullName,       // Maps external field name
            MapTier(response.Level));  // Translates external enum
    }

    private static CustomerTier MapTier(string level) => level switch
    {
        "premium" => CustomerTier.Gold,
        "standard" => CustomerTier.Silver,
        _ => CustomerTier.Regular
    };
}
```

---

## EF Core Configuration for DDD

### Owned Types for Value Objects

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        // Value object as owned type - stored in same table
        builder.OwnsOne(o => o.Total, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 8);
            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(10);
        });

        // Encapsulated collection via backing field
        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Order.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events - not persisted
        builder.Ignore(o => o.DomainEvents);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.OwnsOne(l => l.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPriceAmount")
                .HasPrecision(18, 8);
            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(10);
        });

        builder.Property(l => l.ProductName).HasMaxLength(200);
    }
}
```

### Complex Value Objects as Owned Types

```csharp
// Address as an owned type on Customer
builder.OwnsOne(c => c.ShippingAddress, address =>
{
    address.Property(a => a.Street).HasMaxLength(200);
    address.Property(a => a.City).HasMaxLength(100);
    address.Property(a => a.State).HasMaxLength(50);
    address.Property(a => a.ZipCode).HasMaxLength(20);
    address.Property(a => a.Country).HasMaxLength(100);
});

// Collection of value objects
builder.OwnsMany(c => c.Addresses, address =>
{
    address.WithOwner().HasForeignKey("CustomerId");
    address.Property(a => a.Street).HasMaxLength(200);
    address.ToTable("CustomerAddresses");
});
```

---

## Domain Event Handlers

```csharp
// MediatR notification handler
public sealed class OrderPlacedHandler(
    INotificationService notifications,
    ILogger<OrderPlacedHandler> logger) : INotificationHandler<OrderPlaced>
{
    public async Task Handle(OrderPlaced @event, CancellationToken ct)
    {
        logger.LogInformation(
            "Order {OrderId} placed with total {Total}",
            @event.OrderId, @event.Total);

        await notifications.SendAsync(
            $"New order placed: {@event.OrderId}", ct);
    }
}

// Multiple handlers for the same event - decoupled side effects
public sealed class OrderPlacedInventoryHandler(
    IInventoryService inventory) : INotificationHandler<OrderPlaced>
{
    public async Task Handle(OrderPlaced @event, CancellationToken ct)
    {
        await inventory.ReserveStockAsync(@event.OrderId, ct);
    }
}
```

---

## Common Anti-Patterns to Avoid

### 1. Anemic Domain Model

```csharp
// BAD - anemic: entity is just a data bag, logic in service
public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderService
{
    public void PlaceOrder(Order order)
    {
        if (order.Status != OrderStatus.Draft) throw new Exception("...");
        if (order.Lines.Count == 0) throw new Exception("...");
        order.Status = OrderStatus.Placed;
    }
}

// GOOD - rich model: behavior lives with the data
public class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = [];
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be placed.");
        if (_lines.Count == 0)
            throw new DomainException("Cannot place an empty order.");

        Status = OrderStatus.Placed;
        RaiseDomainEvent(new OrderPlaced(Id, Total.Amount));
    }
}
```

### 2. Aggregate Too Large

```csharp
// BAD - Customer aggregate holds everything
public class Customer : AggregateRoot
{
    public List<Order> Orders { get; set; } = [];       // Should be separate aggregate
    public List<Payment> Payments { get; set; } = [];    // Should be separate aggregate
    public List<Review> Reviews { get; set; } = [];      // Should be separate aggregate
}

// GOOD - Customer only holds its own data, references others by ID
public class Customer : AggregateRoot
{
    public string Name { get; private set; }
    public Address ShippingAddress { get; private set; }
    public CustomerTier Tier { get; private set; }
    // No Orders, Payments, or Reviews here
}
```

### 3. Primitive Obsession

```csharp
// BAD - string and decimal everywhere
public void CreateOrder(string customerId, decimal amount, string currency) { }

// GOOD - explicit domain types
public void CreateOrder(CustomerId customerId, Money total) { }
```

### 4. Modifying Aggregates Outside Their Boundary

```csharp
// BAD - reaching into aggregate internals
order.Lines.Add(new OrderLine { ... });
order.Status = OrderStatus.Placed;

// GOOD - go through aggregate methods
order.AddLine(productId, name, unitPrice, quantity);
order.Place();
```

---

## Testing Domain Models

Domain models are highly testable because they have no infrastructure dependencies.

```csharp
public class OrderTests
{
    [Fact]
    public void Create_ShouldRaiseOrderCreatedEvent()
    {
        var order = Order.Create(CustomerId.New(), "USD");

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreated>();
    }

    [Fact]
    public void Place_WithLines_ShouldChangeStatusToPlaced()
    {
        var order = Order.Create(CustomerId.New(), "USD");
        order.AddLine(ProductId.New(), "Widget", new Money(10, "USD"), 2);

        order.Place();

        order.Status.Should().Be(OrderStatus.Placed);
        order.DomainEvents.Should().HaveCount(2); // Created + Placed
    }

    [Fact]
    public void Place_WithNoLines_ShouldThrow()
    {
        var order = Order.Create(CustomerId.New(), "USD");

        var act = () => order.Place();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot place an empty order.");
    }

    [Fact]
    public void AddLine_WhenNotDraft_ShouldThrow()
    {
        var order = Order.Create(CustomerId.New(), "USD");
        order.AddLine(ProductId.New(), "Widget", new Money(10, "USD"), 1);
        order.Place();

        var act = () => order.AddLine(
            ProductId.New(), "Gadget", new Money(20, "USD"), 1);

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot modify a non-draft order.");
    }

    [Fact]
    public void Total_ShouldSumAllLines()
    {
        var order = Order.Create(CustomerId.New(), "USD");
        order.AddLine(ProductId.New(), "A", new Money(10, "USD"), 2);
        order.AddLine(ProductId.New(), "B", new Money(5, "USD"), 3);

        order.Total.Amount.Should().Be(35); // (10*2) + (5*3)
    }
}

public class MoneyTests
{
    [Fact]
    public void Add_SameCurrency_ShouldSum()
    {
        var a = new Money(10, "USD");
        var b = new Money(20, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(30);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var usd = new Money(10, "USD");
        var eur = new Money(20, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<InvalidOperationException>();
    }
}
```

---

## Checklist

When implementing DDD in .NET, verify:

- [ ] Aggregates enforce their invariants through methods, not public setters
- [ ] Value objects are immutable records with structural equality
- [ ] Domain events are raised inside aggregates, dispatched on save
- [ ] Repositories exist only for aggregate roots
- [ ] Cross-aggregate references are by ID, not object reference
- [ ] Domain layer has zero infrastructure dependencies
- [ ] Ubiquitous language is reflected in class/method names
- [ ] Business rules are tested with pure unit tests (no mocks needed)
- [ ] EF Core configuration uses owned types for value objects
- [ ] Collections are encapsulated (expose `IReadOnlyList`, mutate internally)
