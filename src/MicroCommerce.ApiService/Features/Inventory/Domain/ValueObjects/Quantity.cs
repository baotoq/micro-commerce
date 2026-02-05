using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

/// <summary>
/// Value object representing a non-negative quantity.
/// </summary>
public sealed class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity From(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(value));

        return new Quantity(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
