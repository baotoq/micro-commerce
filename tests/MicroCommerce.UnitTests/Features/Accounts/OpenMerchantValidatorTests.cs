using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class OpenMerchantValidatorTests
{
    private readonly OpenMerchantValidator _validator = new();

    internal static PickupAddressDto Address(string street = "12 Le Loi", string district = "District 1", string province = "Ho Chi Minh City") =>
        new(street, district, province);

    [Fact]
    public void A_complete_request_is_valid() =>
        _validator.Validate(new OpenMerchantRequest("Sunny Mugs", "Hand-thrown mugs", Address())).IsValid.ShouldBeTrue();

    [Fact]
    public void Description_is_optional() =>
        _validator.Validate(new OpenMerchantRequest("Sunny Mugs", null, Address())).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ab ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public void Shop_name_must_be_3_to_50_characters_after_trimming(string? name) =>
        Errors(new OpenMerchantRequest(name!, null, Address())).ShouldBe(["ShopName"]);

    [Fact]
    public void Description_is_at_most_2000_characters() =>
        Errors(new OpenMerchantRequest("Sunny Mugs", new string('d', 2001), Address())).ShouldBe(["Description"]);

    [Fact]
    public void Pickup_address_is_required() =>
        Errors(new OpenMerchantRequest("Sunny Mugs", null, null!)).ShouldBe(["PickupAddress"]);

    [Theory]
    [InlineData("", "District 1", "Province", "PickupAddress.Street")]
    [InlineData("Street", "", "Province", "PickupAddress.District")]
    [InlineData("Street", "District 1", "", "PickupAddress.Province")]
    public void Every_address_part_is_required(string street, string district, string province, string field) =>
        Errors(new OpenMerchantRequest("Sunny Mugs", null, Address(street, district, province))).ShouldBe([field]);

    [Fact]
    public void Address_parts_have_maximum_lengths()
    {
        Errors(new OpenMerchantRequest("Sunny Mugs", null, Address(street: new string('s', 201)))).ShouldBe(["PickupAddress.Street"]);
        Errors(new OpenMerchantRequest("Sunny Mugs", null, Address(district: new string('d', 101)))).ShouldBe(["PickupAddress.District"]);
        Errors(new OpenMerchantRequest("Sunny Mugs", null, Address(province: new string('p', 101)))).ShouldBe(["PickupAddress.Province"]);
    }

    private List<string> Errors(OpenMerchantRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.PropertyName).Distinct().ToList();
}
