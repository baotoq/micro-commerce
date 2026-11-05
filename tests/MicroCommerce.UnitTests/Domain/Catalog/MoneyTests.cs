using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class MoneyTests
{
    [Theory]
    [InlineData(1L)]
    [InlineData(120_000L)]
    [InlineData(Money.MaxValue)]
    public void Whole_amounts_above_zero_up_to_the_javascript_safe_limit_are_valid(long amount) =>
        Money.From(amount).Value.ShouldBe(amount);

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(Money.MaxValue + 1)]
    public void Zero_negative_and_unsafe_amounts_are_refused(long amount) =>
        Should.Throw<DomainRuleViolation>(() => Money.From(amount));

    [Fact]
    public void The_limit_is_javascripts_max_safe_integer() => Money.MaxValue.ShouldBe(9_007_199_254_740_991L);
}
