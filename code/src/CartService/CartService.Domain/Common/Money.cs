using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Common;

public class Money : ValueObject
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        Amount = amount;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString()
    {
        return $"{Amount}";
    }
}
