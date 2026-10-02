using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class OpenMerchantValidatorTests
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");

    private readonly OpenMerchantValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.Validate(new OpenMerchantRequest("Mug Corner", Address));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_shop_name_fails(string shopName)
    {
        var result = _validator.Validate(new OpenMerchantRequest(shopName, Address));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(OpenMerchantRequest.ShopName));
    }

    [Fact]
    public void Shop_name_longer_than_100_characters_fails()
    {
        var result = _validator.Validate(new OpenMerchantRequest(new string('a', 101), Address));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(OpenMerchantRequest.ShopName));
    }

    [Fact]
    public void Missing_pickup_address_fails()
    {
        var result = _validator.Validate(new OpenMerchantRequest("Mug Corner", null!));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(OpenMerchantRequest.PickupAddress));
    }

    [Theory]
    [InlineData("", "Ben Nghe", "District 1", "Ho Chi Minh City", "PickupAddress.Street")]
    [InlineData("12 Le Loi", " ", "District 1", "Ho Chi Minh City", "PickupAddress.Ward")]
    [InlineData("12 Le Loi", "Ben Nghe", "", "Ho Chi Minh City", "PickupAddress.District")]
    [InlineData("12 Le Loi", "Ben Nghe", "District 1", "", "PickupAddress.Province")]
    public void Incomplete_pickup_address_fails(string street, string ward, string district, string province, string property)
    {
        var result = _validator.Validate(new OpenMerchantRequest("Mug Corner", new(street, ward, district, province)));

        result.Errors.Select(e => e.PropertyName).ShouldBe([property]);
    }
}
