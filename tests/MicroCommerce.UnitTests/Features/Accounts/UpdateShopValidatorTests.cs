using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class UpdateShopValidatorTests
{
    private readonly UpdateShopValidator _validator = new();

    [Fact]
    public void A_complete_request_is_valid() =>
        _validator.Validate(new UpdateShopRequest("Sunny Mugs", null, OpenMerchantValidatorTests.Address(), 1)).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Version_is_required_and_positive(int? version) =>
        _validator.Validate(new UpdateShopRequest("Sunny Mugs", null, OpenMerchantValidatorTests.Address(), version))
            .Errors.Select(e => e.PropertyName).Distinct().ShouldBe(["Version"]);

    [Fact]
    public void Profile_rules_match_opening_a_merchant() =>
        _validator.Validate(new UpdateShopRequest("ab", null, OpenMerchantValidatorTests.Address(province: ""), 1))
            .Errors.Select(e => e.PropertyName).Distinct().Order().ShouldBe(["PickupAddress.Province", "ShopName"]);
}
