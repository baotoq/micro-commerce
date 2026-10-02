using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class UpdateShopValidatorTests
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");

    private readonly UpdateShopValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("Mugs and teapots.")]
    public void Valid_request_passes(string? description)
    {
        var result = _validator.Validate(new UpdateShopRequest("Mug Corner", description, Address));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Empty_shop_name_fails()
    {
        var result = _validator.Validate(new UpdateShopRequest(" ", null, Address));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateShopRequest.ShopName));
    }

    [Fact]
    public void Description_longer_than_2000_characters_fails()
    {
        var result = _validator.Validate(new UpdateShopRequest("Mug Corner", new string('a', 2001), Address));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateShopRequest.Description));
    }

    [Fact]
    public void Incomplete_pickup_address_fails()
    {
        var result = _validator.Validate(new UpdateShopRequest("Mug Corner", null, Address with { Province = "" }));

        result.Errors.Select(e => e.PropertyName).ShouldBe(["PickupAddress.Province"]);
    }
}
