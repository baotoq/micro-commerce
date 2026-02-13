namespace MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;

/// <summary>
/// ReviewText value object with validation (10-1000 chars).
/// </summary>
public sealed record ReviewText
{
    public string Value { get; }

    private ReviewText(string value) => Value = value;

    public static ReviewText Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var trimmed = value.Trim();
        if (trimmed.Length < 10)
            throw new ArgumentException("Review text must be at least 10 characters.", nameof(value));
        if (trimmed.Length > 1000)
            throw new ArgumentException("Review text must not exceed 1000 characters.", nameof(value));
        return new ReviewText(trimmed);
    }
}
