using MicroCommerce.ApiService.Features.Catalog;
using Vogen;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(120_000)]
    [InlineData(long.MaxValue)]
    public void Zero_and_positive_amounts_are_valid(long amount)
    {
        Money.From(amount).Value.ShouldBe(amount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void Negative_amounts_are_invalid(long amount)
    {
        Should.Throw<ValueObjectValidationException>(() => Money.From(amount));
    }

    [Fact]
    public void Amounts_compare_by_value()
    {
        Money.From(50_000).ShouldBe(Money.From(50_000));
        Money.From(50_000).ShouldBeLessThan(Money.From(99_000));
    }
}
