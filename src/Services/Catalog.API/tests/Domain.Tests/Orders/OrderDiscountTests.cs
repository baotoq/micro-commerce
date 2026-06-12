using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Domain.Tests.Orders;

public class OrderDiscountTests
{
    private static Order NewOrder(decimal unitPrice = 86m, int qty = 1) =>
        new(
            1042,
            new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero),
            "Sasha Leblanc", "sasha.l@gmail.com", "San Francisco, CA",
            "820 Sutter St · #4B", "San Francisco, CA 94109", true,
            "Persimmon vase", "Standard", 8m, 6.5m, 4m,
            "Visa", "4242", null, 0m, null, "",
            [new OrderLine("MC-VS-001", "Persimmon vase", "", qty, unitPrice, FulfillmentStatus.Awaiting, null, null)]);

    [Fact]
    public void ApplyDiscount_ReducesPaid()
    {
        var order = NewOrder(unitPrice: 86m); // Subtotal 86, Shipping 8, Tax 6.5
        order.ApplyDiscount("WELCOME10", 10m);

        Assert.Equal("WELCOME10", order.DiscountCode);
        Assert.Equal(10m, order.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.5m, order.Paid);
    }

    [Fact]
    public void NoDiscount_PaidUnchanged()
    {
        var order = NewOrder(unitPrice: 86m);
        Assert.Null(order.DiscountCode);
        Assert.Equal(0m, order.DiscountAmount);
        Assert.Equal(86m + 8m + 6.5m, order.Paid);
    }

    [Fact]
    public void ApplyDiscount_NegativeAmount_Throws()
    {
        var order = NewOrder();
        Assert.Throws<ArgumentOutOfRangeException>(() => order.ApplyDiscount("X", -1m));
    }

    [Fact]
    public void ApplyDiscount_OverSubtotal_Throws()
    {
        var order = NewOrder(unitPrice: 20m);
        Assert.Throws<ArgumentOutOfRangeException>(() => order.ApplyDiscount("X", 21m));
    }

    [Fact]
    public void ApplyDiscount_BlankCode_Throws()
    {
        var order = NewOrder();
        Assert.Throws<ArgumentException>(() => order.ApplyDiscount(" ", 5m));
    }
}
