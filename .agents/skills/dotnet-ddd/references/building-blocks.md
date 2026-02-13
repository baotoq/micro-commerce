# Building Blocks — Entity, Value Object, Aggregate

## Entity Deep Dive

Entities have **identity** — two entities with the same data but different IDs are different objects.

### When to Use an Entity

- The object has a lifecycle (created, modified, deleted)
- You need to track it over time
- Two instances with the same attributes are **not** the same thing
- Examples: `Order`, `User`, `Product`, `Invoice`

### Entity Base Class Variations

**With audit fields:**

```csharp
public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected AuditableEntity(TId id) : base(id)
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

**Soft-deletable:**

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    void MarkDeleted();
}
```

### Child Entities (Non-Root)

Child entities live inside an Aggregate and are not directly accessible from outside:

```csharp
public sealed class OrderLine : Entity<OrderLineId>
{
    public ProductId ProductId { get; private init; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private init; }
    public Money SubTotal => UnitPrice with { Amount = UnitPrice.Amount * Quantity };

    internal OrderLine(OrderLineId id, ProductId productId, int quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be positive");
        Quantity = newQuantity;
    }

    private OrderLine() { } // ORM
}
```

Key: use `internal` constructors so only the Aggregate Root can create child entities.

## Value Object Deep Dive

Value Objects have **no identity** — they are defined entirely by their attributes. Two VOs with the same data are equal.

### When to Use a Value Object

- The concept has no meaningful identity
- Equality is based on all properties
- It should be immutable
- Examples: `Money`, `Address`, `Email`, `PhoneNumber`, `DateRange`, `Coordinate`

### Using `record` (Preferred)

C# `record` types give you structural equality, immutability, and `with` expressions for free:

```csharp
public record Email
{
    public string Value { get; init; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new DomainException($"Invalid email: {value}");
        Value = value.Trim().ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
}

public record Percentage
{
    public decimal Value { get; init; }

    public Percentage(decimal value)
    {
        if (value is < 0 or > 100)
            throw new DomainException("Percentage must be between 0 and 100");
        Value = value;
    }
}
```

### Value Object with Collection

```csharp
public record Tags
{
    private readonly List<string> _values;
    public IReadOnlyList<string> Values => _values;

    public Tags(IEnumerable<string> values)
    {
        _values = values
            .Select(v => v.Trim().ToLowerInvariant())
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }

    public Tags Add(string tag) => new([.._values, tag]);
    public Tags Remove(string tag) => new(_values.Where(v => v != tag.ToLowerInvariant()));
    public bool Contains(string tag) => _values.Contains(tag.ToLowerInvariant());

    // Override equality to compare by content
    public virtual bool Equals(Tags? other) =>
        other is not null && _values.SequenceEqual(other._values);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var v in _values) hash.Add(v);
        return hash.ToHashCode();
    }
}
```

### When `record` Isn't Enough

Use a `sealed class` with manual equality for edge cases (e.g., EF Core value converters in older versions):

```csharp
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Currency mismatch: {Currency} vs {other.Currency}");
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount} {Currency}";
}
```

## Aggregate Deep Dive

An Aggregate is a cluster of domain objects treated as a single unit for data changes. The **Aggregate Root** is the single entry point.

### Aggregate Design Rules

1. **Protect invariants inside the boundary** — all business rules validated in the root
2. **Reference other Aggregates by ID** — never hold a direct reference
3. **One transaction, one Aggregate** — cross-aggregate consistency via Domain Events
4. **Design small Aggregates** — prefer fewer entities per aggregate
5. **Delete by root** — deleting the root cascades to all children

### Complete Aggregate Example

```csharp
public sealed class ShoppingCart : Entity<CartId>
{
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items;
    public CustomerId CustomerId { get; private init; }

    public Money Total => _items
        .Aggregate(Money.Zero("USD"), (sum, item) => sum.Add(item.SubTotal));

    private ShoppingCart() { }

    public static ShoppingCart Create(CustomerId customerId) =>
        new(CartId.New()) { CustomerId = customerId };

    public void AddItem(ProductId productId, Money unitPrice, int quantity = 1)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        var existing = _items.Find(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(CartItemId.New(), productId, unitPrice, quantity));
        }
    }

    public void RemoveItem(ProductId productId)
    {
        var item = _items.Find(i => i.ProductId == productId)
            ?? throw new DomainException($"Product {productId} not in cart");
        _items.Remove(item);
    }

    public void Clear() => _items.Clear();

    public Order Checkout()
    {
        if (_items.Count == 0)
            throw new DomainException("Cannot checkout empty cart");

        var order = Order.Create(CustomerId);
        foreach (var item in _items)
        {
            order.AddLine(item.ProductId, item.Quantity, item.UnitPrice);
        }

        Clear();
        RaiseDomainEvent(new CartCheckedOutEvent(Id, order.Id));
        return order;
    }
}
```

### Aggregate Boundaries Checklist

Ask these questions to find the right boundary:

- [ ] What data must be **immediately consistent** together?
- [ ] What is the minimum cluster that enforces an invariant?
- [ ] Could this child entity be its own Aggregate referenced by ID?
- [ ] Would splitting reduce contention in concurrent scenarios?
- [ ] Is the Aggregate small enough to load in a single query?

### Common Mistake: Too-Large Aggregates

```csharp
// BAD: Customer contains everything
public class Customer : Entity<CustomerId>
{
    public List<Order> Orders { get; set; }       // Should be separate aggregate
    public List<Address> Addresses { get; set; }  // Could be VO collection
    public ShoppingCart Cart { get; set; }         // Should be separate aggregate
}

// GOOD: Customer is focused
public sealed class Customer : Entity<CustomerId>
{
    public string Name { get; private set; }
    public Email Email { get; private set; }

    // Reference other aggregates by ID
    public CartId? ActiveCartId { get; private set; }
}
```

## Domain Exceptions

Use a dedicated exception type for domain rule violations:

```csharp
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

Or use the Result pattern (see [patterns.md](patterns.md)) for expected failures where exceptions feel too heavy.
