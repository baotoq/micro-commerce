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

    [Fact]
    public void New_merchant_starts_at_profile_version_one_and_its_opened_event_carries_its_profile()
    {
        var merchant = NewMerchant("Mug Corner");

        merchant.ProfileVersion.ShouldBe(1);
        merchant.ToOpenedEvent().ShouldBe(new MerchantOpened(merchant.Id, "Mug Corner", null, 1));
    }

    [Fact]
    public void Changing_the_profile_moves_to_the_next_version()
    {
        var merchant = NewMerchant("Mug Corner");
        var address = new PickupAddress("1 Tran Phu", "Hai Chau 1", "Hai Chau", "Da Nang");

        merchant.ChangeProfile(ShopName.From("Tea House"), "Teapots.", address);
        merchant.ChangeProfile(ShopName.From("Tea House"), "Teapots and cups.", address);

        (merchant.ShopName, merchant.Description, merchant.PickupAddress).ShouldBe((ShopName.From("Tea House"), "Teapots and cups.", address));
        merchant.NormalizedShopName.ShouldBe(ShopName.From("tea house").Normalized);
        merchant.ToProfileChangedEvent().ShouldBe(new MerchantProfileChanged(merchant.Id, "Tea House", "Teapots and cups.", 3));
    }
}
