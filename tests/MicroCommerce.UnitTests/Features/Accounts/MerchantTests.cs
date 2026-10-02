using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class MerchantTests
{
    private static Merchant NewMerchant(string shopName) => new()
    {
        AccountId = Guid.CreateVersion7(),
        ShopName = shopName,
        PickupAddress = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City"),
    };

    [Fact]
    public void Shop_names_differing_only_in_case_normalize_the_same()
    {
        NewMerchant("Mug Corner").NormalizedShopName.ShouldBe(NewMerchant("MUG corner").NormalizedShopName);
    }

    [Fact]
    public void Vietnamese_shop_names_normalize_ignoring_case()
    {
        NewMerchant("Cửa hàng Đà Nẵng").NormalizedShopName.ShouldBe(NewMerchant("CỬA HÀNG đà nẵng").NormalizedShopName);
    }

    [Fact]
    public void Renaming_updates_the_normalized_shop_name()
    {
        var merchant = NewMerchant("Mug Corner");

        merchant.ShopName = "Tea House";

        merchant.NormalizedShopName.ShouldBe(NewMerchant("tea house").NormalizedShopName);
    }
}
