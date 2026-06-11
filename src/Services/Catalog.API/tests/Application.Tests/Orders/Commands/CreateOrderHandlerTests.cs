using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class CreateOrderHandlerTests
{
    private static CreateOrderCommand Cmd(int number = 1042) =>
        new(
            Number: number,
            PlacedAt: new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            CustomerName: "Sasha Leblanc",
            CustomerEmail: "sasha.l@gmail.com",
            CityState: "San Francisco, CA",
            ShipLine1: "820 Sutter St · #4B",
            ShipLine2: "San Francisco, CA 94109",
            BillSameAsShip: true,
            ItemsSummary: "Persimmon vase, Ash budstem",
            ShippingMethod: "USPS Priority",
            ShippingPaid: 0m,
            Tax: 0m,
            FeePct: 4m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4421",
            LabelCarrier: "USPS",
            LabelCost: 9.84m,
            LabelWeightLabel: "1lb 4oz",
            InternalNote: "",
            Lines:
            [
                new CreateOrderLineInput("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, "shipped", "USPS · 9405", null),
                new CreateOrderLineInput("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, "awaiting", null, "back in stock Tue"),
            ]);

    [Fact]
    public async Task DuplicateNumber_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateOrderHandler(db, publisher);
        await handler.Handle(Cmd(), TestContext.Current.CancellationToken);
        publisher.Published.Clear();

        var result = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_NUMBER_DUPLICATE", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task Valid_PersistsAndPublishesCreatedEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateOrderHandler(db, publisher);

        var result = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1042, result.Value!.Number);
        Assert.Equal("new", result.Value.Status);
        // Paid = 86 + 66 = 152
        Assert.Equal(152.00m, result.Value.Summary.Paid);
        // Net = 152 - 6.08 - 9.84 = 136.08
        Assert.Equal(136.08m, result.Value.Summary.Net);
        Assert.Single(db.Orders);

        var evt = Assert.IsType<OrderCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(1042, evt.Number);
        Assert.Equal("new", evt.Status);
    }
}
