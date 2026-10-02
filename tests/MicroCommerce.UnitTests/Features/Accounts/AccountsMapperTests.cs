using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class AccountsMapperTests
{
    private static readonly Account Account = new() { Subject = "sub-1", Email = "buyer@example.com", DisplayName = "Buyer" };

    private static readonly Merchant Merchant = new()
    {
        AccountId = Account.Id,
        ShopName = "Mug Corner",
        Description = "Mugs, mostly.",
        PickupAddress = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City"),
    };

    [Fact]
    public void Merchant_response_maps_all_fields()
    {
        var response = Merchant.ToResponse();

        response.ShouldBe(new MerchantResponse(
            Merchant.Id, "Mug Corner", "Mugs, mostly.", Merchant.PickupAddress, Merchant.CreatedAt));
    }

    [Fact]
    public void Me_response_maps_the_account_and_its_merchant()
    {
        var response = Account.ToMeResponse(isPlatformOperator: true, Merchant);

        response.ShouldBe(new MeResponse(Account.Id, "buyer@example.com", "Buyer", true, Merchant.ToResponse()));
    }

    [Fact]
    public void Me_response_has_no_merchant_when_the_account_owns_none()
    {
        var response = Account.ToMeResponse(isPlatformOperator: false, merchant: null);

        response.ShouldBe(new MeResponse(Account.Id, "buyer@example.com", "Buyer", false, null));
    }
}
