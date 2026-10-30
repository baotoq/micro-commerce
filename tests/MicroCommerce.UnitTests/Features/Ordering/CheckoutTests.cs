using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Ordering;

public class CheckoutTests
{
    private readonly DeliveryAddressValidator _addressValidator = new();

    [Fact]
    public void The_province_list_has_34_unique_units_and_no_districts()
    {
        VietnamProvinces.All.Count.ShouldBe(34);
        VietnamProvinces.All.Distinct().Count().ShouldBe(34);
        VietnamProvinces.All.ShouldContain("Hà Nội");
        VietnamProvinces.All.ShouldContain("Hồ Chí Minh");
        VietnamProvinces.All.ShouldNotContain("District 1");
    }

    [Fact]
    public void A_delivery_address_with_an_official_province_is_valid()
    {
        _addressValidator.Validate(Address("Hà Nội")).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void A_delivery_address_with_an_unsupported_province_is_invalid()
    {
        _addressValidator.Validate(Address("District 1")).IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void A_delivery_address_requires_a_recipient(string recipient)
    {
        _addressValidator.Validate(Address("Hà Nội") with { RecipientName = recipient }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void An_awaiting_checkout_expires_once()
    {
        var checkout = CreateCheckout();

        checkout.Status.ShouldBe(CheckoutStatus.AwaitingPayment);
        checkout.Expire().ShouldBeTrue();
        checkout.Status.ShouldBe(CheckoutStatus.Expired);
        checkout.Expire().ShouldBeFalse();
        checkout.Status.ShouldBe(CheckoutStatus.Expired);
    }

    [Fact]
    public void An_awaiting_checkout_becomes_paid_once()
    {
        var checkout = CreateCheckout();

        checkout.MarkPaid().ShouldBeTrue();
        checkout.Status.ShouldBe(CheckoutStatus.Paid);
        checkout.MarkPaid().ShouldBeFalse();
        checkout.Expire().ShouldBeFalse();
        checkout.Status.ShouldBe(CheckoutStatus.Paid);
    }

    [Fact]
    public void An_order_is_created_for_a_checkout_merchant_group()
    {
        var group = new CheckoutMerchant
        {
            CheckoutId = CheckoutId.New(),
            MerchantId = MerchantId.New(),
            Subtotal = Money.From(120_000),
            ShippingFee = Money.From(30_000),
        };

        var order = Order.FromGroup(group);

        order.Id.Value.ShouldNotBe(Guid.Empty);
        order.CheckoutId.ShouldBe(group.CheckoutId);
        order.MerchantId.ShouldBe(group.MerchantId);
        order.Status.ShouldBe(OrderStatus.ToShip);
    }

    private static DeliveryAddress Address(string province) => new()
    {
        RecipientName = "Buyer",
        Phone = "+84901234567",
        Street = "12 Le Loi",
        Ward = "Ben Nghe",
        Province = province,
    };

    private static Checkout CreateCheckout() => new()
    {
        Id = CheckoutId.New(),
        AccountId = AccountId.New(),
        PlacedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
        DeliveryAddress = Address("Hà Nội"),
        ItemsTotal = Money.From(100_000),
        ShippingTotal = Money.From(30_000),
        Total = Money.From(130_000),
    };
}
