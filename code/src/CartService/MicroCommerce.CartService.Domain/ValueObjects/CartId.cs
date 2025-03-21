using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.ValueObjects;

public sealed class CartId : ValueObject
{
    public Guid Value { get; }

    private CartId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CartId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static CartId New() => new(Guid.CreateVersion7());

    public static CartId From(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
