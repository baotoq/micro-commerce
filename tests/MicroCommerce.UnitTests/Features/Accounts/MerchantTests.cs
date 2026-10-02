using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class MerchantTests
{
    private static Merchant NewMerchant(string shopName) => new()
    {
        AccountId = AccountId.New(),
        ShopName = ShopName.From(shopName),
        PickupAddress = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City"),
    };

    [Fact]
    public void New_merchant_stores_the_normalized_shop_name()
    {
        NewMerchant("Mug Corner").NormalizedShopName.ShouldBe(ShopName.From("mug corner").Normalized);
    }

    [Fact]
    public void Renaming_updates_the_normalized_shop_name()
    {
        var merchant = NewMerchant("Mug Corner");

        merchant.ShopName = ShopName.From("Tea House");

        merchant.NormalizedShopName.ShouldBe(ShopName.From("tea house").Normalized);
    }
}
