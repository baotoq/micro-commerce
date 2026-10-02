using MicroCommerce.ApiService.Features.Accounts;
using Vogen;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class ShopNameTests
{
    [Fact]
    public void Surrounding_whitespace_is_trimmed()
    {
        ShopName.From("  Mug Corner  ").Value.ShouldBe("Mug Corner");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_shop_name_is_invalid(string value)
    {
        Should.Throw<ValueObjectValidationException>(() => ShopName.From(value));
    }

    [Fact]
    public void Shop_name_of_100_characters_is_valid()
    {
        ShopName.From(new string('a', 100)).Value.Length.ShouldBe(100);
    }

    [Fact]
    public void Shop_name_over_100_characters_is_invalid()
    {
        Should.Throw<ValueObjectValidationException>(() => ShopName.From(new string('a', 101)));
    }

    [Fact]
    public void Length_is_checked_after_trimming()
    {
        ShopName.From($" {new string('a', 100)} ").Value.Length.ShouldBe(100);
    }

    [Fact]
    public void TryFrom_reports_an_invalid_shop_name_without_throwing()
    {
        ShopName.TryFrom("", out _).ShouldBeFalse();
    }

    [Fact]
    public void Shop_names_differing_only_in_case_normalize_the_same()
    {
        ShopName.From("Mug Corner").Normalized.ShouldBe(ShopName.From("MUG corner").Normalized);
    }

    [Fact]
    public void Vietnamese_shop_names_normalize_ignoring_case()
    {
        ShopName.From("Cửa hàng Đà Nẵng").Normalized.ShouldBe(ShopName.From("CỬA HÀNG đà nẵng").Normalized);
    }

    [Fact]
    public void Different_shop_names_normalize_differently()
    {
        ShopName.From("Mug Corner").Normalized.ShouldNotBe(ShopName.From("Tea House").Normalized);
    }
}
