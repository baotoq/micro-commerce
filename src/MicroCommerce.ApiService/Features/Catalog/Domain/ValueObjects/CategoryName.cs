using Ardalis.GuardClauses;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a category name with validation.
/// </summary>
public readonly record struct CategoryName
{
    public const int MinLength = 2;
    public const int MaxLength = 100;

    public string Value { get; init; }

    private CategoryName(string value)
    {
        Value = value;
    }

    public static CategoryName Create(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        var trimmed = value.Trim();
        Guard.Against.LengthOutOfRange(trimmed, MinLength, MaxLength, nameof(value));

        return new CategoryName(trimmed);
    }

    public override string ToString() => Value;

    // Implicit conversion for convenience
    public static implicit operator string(CategoryName name) => name.Value;
}
