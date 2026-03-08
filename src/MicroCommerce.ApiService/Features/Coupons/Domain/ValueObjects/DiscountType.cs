using Ardalis.SmartEnum;

namespace MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;

public abstract class DiscountType : SmartEnum<DiscountType>
{
    public static readonly DiscountType Percentage  = new PercentageType();
    public static readonly DiscountType FixedAmount = new FixedAmountType();

    private DiscountType(string name, int value) : base(name, value) { }

    public abstract decimal Calculate(decimal subtotal, decimal discountValue, decimal? maxDiscountAmount);

    private sealed class PercentageType : DiscountType
    {
        public PercentageType() : base("Percentage", 1) { }

        public override decimal Calculate(decimal subtotal, decimal discountValue, decimal? maxDiscountAmount)
        {
            decimal discount = subtotal * discountValue / 100m;
            if (maxDiscountAmount.HasValue)
                discount = Math.Min(discount, maxDiscountAmount.Value);
            return Math.Round(discount, 2);
        }
    }

    private sealed class FixedAmountType : DiscountType
    {
        public FixedAmountType() : base("FixedAmount", 2) { }

        public override decimal Calculate(decimal subtotal, decimal discountValue, decimal? maxDiscountAmount) =>
            Math.Min(discountValue, subtotal);
    }
}
