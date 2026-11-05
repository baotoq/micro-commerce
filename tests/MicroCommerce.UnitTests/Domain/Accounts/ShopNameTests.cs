using MicroCommerce.Domain.Accounts;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Accounts;

public class ShopNameTests
{
    [Fact]
    public void Is_trimmed() => ShopName.From("  Sunny Mugs  ").Value.ShouldBe("Sunny Mugs");

    [Theory]
    [InlineData("abc")]
    [InlineData("12345678901234567890123456789012345678901234567890")]
    public void Accepts_3_to_50_characters(string name) => ShopName.From(name).Value.ShouldBe(name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData(" ab ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public void Refuses_fewer_than_3_or_more_than_50_characters(string name) =>
        Should.Throw<DomainRuleViolation>(() => ShopName.From(name));

    [Fact]
    public void Normalized_form_ignores_letter_case() =>
        ShopName.From("Sunny Mugs").Normalized.ShouldBe(ShopName.From("sunny MUGS").Normalized);
}
