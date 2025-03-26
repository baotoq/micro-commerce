using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ProductService.Domain.ValueObjects;

public class Price : ValueObject
{
    public decimal Amount { get; }

    public Price(decimal amount)
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
