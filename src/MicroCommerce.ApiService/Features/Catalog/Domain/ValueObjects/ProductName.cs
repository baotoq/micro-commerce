using Ardalis.GuardClauses;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a product name with validation.
/// </summary>
public readonly record struct ProductName
{
    public const int MinLength = 2;
    public const int MaxLength = 200;

    public string Value { get; init; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        var trimmed = value.Trim();
        Guard.Against.LengthOutOfRange(trimmed, MinLength, MaxLength, nameof(value));

        return new ProductName(trimmed);
    }

    public override string ToString() => Value;

    // Implicit conversion for convenience
    public static implicit operator string(ProductName name) => name.Value;
}
