using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

public sealed class DisplayName : ValueObject
{
    public string Value { get; private set; }

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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
