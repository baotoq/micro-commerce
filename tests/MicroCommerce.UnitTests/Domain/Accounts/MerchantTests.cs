using MicroCommerce.Domain.Accounts;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Accounts;

public class MerchantTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly PickupAddress Address = new("12 Le Loi", "District 1", "Ho Chi Minh City");

    private static Merchant Open(string name = "Sunny Mugs", string? description = "Hand-thrown mugs") =>
        Merchant.Open(MerchantId.New(), AccountId.From(Guid.NewGuid()), ShopName.From(name), description, Address, Now);

    [Fact]
    public void Opening_records_the_owner_profile_and_version_1()
    {
        var id = MerchantId.New();
        var owner = AccountId.From(Guid.NewGuid());

        var merchant = Merchant.Open(id, owner, ShopName.From("Sunny Mugs"), "Hand-thrown mugs", Address, Now);

        merchant.Id.ShouldBe(id);
        merchant.AccountId.ShouldBe(owner);
        merchant.ShopName.Value.ShouldBe("Sunny Mugs");
        merchant.NormalizedShopName.ShouldBe("SUNNY MUGS");
        merchant.Description.ShouldBe("Hand-thrown mugs");
        merchant.PickupAddress.ShouldBe(Address);
        merchant.CreatedAt.ShouldBe(Now);
        merchant.Version.ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_description_becomes_null(string? description) => Open(description: description).Description.ShouldBeNull();

    [Fact]
    public void A_description_is_at_most_2000_characters()
    {
        Open(description: new string('d', 2000)).Description!.Length.ShouldBe(2000);
        Should.Throw<DomainRuleViolation>(() => Open(description: new string('d', 2001)));
    }

    [Fact]
    public void Changing_the_profile_bumps_the_version()
    {
        var merchant = Open();
        var newAddress = new PickupAddress("1 Tran Hung Dao", "District 5", "Ho Chi Minh City");

        merchant.ChangeProfile(ShopName.From("Sunny Cups"), "Cups too", newAddress);

        merchant.ShopName.Value.ShouldBe("Sunny Cups");
        merchant.NormalizedShopName.ShouldBe("SUNNY CUPS");
        merchant.Description.ShouldBe("Cups too");
        merchant.PickupAddress.ShouldBe(newAddress);
        merchant.Version.ShouldBe(2);
    }

    [Fact]
    public void A_case_only_rename_keeps_the_normalized_name()
    {
        var merchant = Open("Sunny Mugs");

        merchant.ChangeProfile(ShopName.From("SUNNY mugs"), null, Address);

        merchant.ShopName.Value.ShouldBe("SUNNY mugs");
        merchant.NormalizedShopName.ShouldBe("SUNNY MUGS");
    }
}
