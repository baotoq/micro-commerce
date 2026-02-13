namespace MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;

/// <summary>
/// Rating value object with validation (1-5).
/// </summary>
public sealed record Rating
{
    public int Value { get; }

    private Rating(int value) => Value = value;

    public static Rating Create(int value)
    {
        if (value < 1 || value > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");
        return new Rating(value);
    }
}
