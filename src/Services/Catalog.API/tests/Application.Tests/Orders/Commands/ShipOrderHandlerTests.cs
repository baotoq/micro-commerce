using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class ShipOrderHandlerTests
{
    [Fact]
    public async Task FromPacked_PersistsShipped_AndMarksLinesShipped()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Orders.Add(TestOrders.CreatePacked(number: 2010));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new ShipOrderHandler(act, publisher);
            var result = await handler.Handle(new ShipOrderCommand(2010, "USPS", "USPS · 1Z999"), TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess);
            Assert.Equal("shipped", result.Value!.Status);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = Assert.Single(assert.Orders);
            Assert.Equal(OrderStatus.Shipped, stored.Status);
            Assert.All(stored.Lines, l => Assert.Equal(FulfillmentStatus.Shipped, l.FulfillmentStatus));
        }

        var evt = Assert.IsType<OrderStatusChangedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("shipped", evt.Status);
    }

    [Fact]
    public async Task UnknownNumber_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var handler = new ShipOrderHandler(db, new FakePublisher());

        var result = await handler.Handle(new ShipOrderCommand(9999, "USPS", null), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_NOT_FOUND", result.Error!.Value.Code);
    }
}
