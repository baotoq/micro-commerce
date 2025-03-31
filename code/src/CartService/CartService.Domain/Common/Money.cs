using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Common;

public class Money(decimal amount) : ValueObject
{
    public static readonly Money Zero = new Money(0);

    public decimal Amount { get; } = amount;

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Amount - right.Amount);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier);
    }

    public static Money operator *(decimal multiplier, Money money)
    {
        return money * multiplier;
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
        return new Money(money.Amount / divisor);
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
