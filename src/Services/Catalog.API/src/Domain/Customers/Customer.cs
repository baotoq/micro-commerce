namespace MicroCommerce.Catalog.Domain.Customers;

public class Customer
{
    private const int MaxNameLength = 120;
    private const int MaxTags = 10;
    private const int MaxTagLength = 50;

    private readonly List<string> _tags = [];

    public CustomerId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public string City { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    // Tags stored as an owned collection of simple strings (no separate entity).
    public IReadOnlyList<string> Tags => _tags;

    // LifetimeOrderCount and LifetimeSpend are NOT stored fields —
    // they are derived at query time by grouping db.Orders on CustomerEmail.

    private Customer() { }

    public Customer(string name, Email email, string city, DateTimeOffset createdAt)
    {
        ValidateName(name);
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City required.", nameof(city));

        Id = CustomerId.New();
        Name = name;
        Email = email;
        City = city;
        CreatedAt = createdAt;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new InvalidOperationException("Tag cannot be empty.");
        if (tag.Length > MaxTagLength)
            throw new InvalidOperationException($"Tag must be {MaxTagLength} characters or fewer.");

        if (_tags.Contains(tag))
            return;

        if (_tags.Count >= MaxTags)
            throw new InvalidOperationException($"Cannot add more than {MaxTags} tags.");

        _tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        _tags.Remove(tag);
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required.", nameof(name));
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name must be {MaxNameLength} characters or fewer.", nameof(name));
    }
}
