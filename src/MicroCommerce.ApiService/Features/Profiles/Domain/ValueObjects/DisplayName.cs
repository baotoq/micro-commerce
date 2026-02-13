namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

/// <summary>
/// Value object representing a user's display name with validation.
/// </summary>
public readonly record struct DisplayName
{
    public string Value { get; init; }

    private DisplayName(string value)
    {
        Value = value;
    }

    public static DisplayName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Display name cannot be null or empty.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 50)
            throw new ArgumentException("Display name must be between 2 and 50 characters.", nameof(value));

        return new DisplayName(trimmed);
    }

    public override string ToString() => Value;
}
