using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class DeliverOrderHandlerTests
{
    [Fact]
    public async Task FromShipped_PersistsDelivered()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Orders.Add(TestOrders.CreateShipped(number: 2020));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new DeliverOrderHandler(act, publisher);
            var result = await handler.Handle(new DeliverOrderCommand(2020), TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess);
            Assert.Equal("delivered", result.Value!.Status);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = Assert.Single(assert.Orders);
            Assert.Equal(OrderStatus.Delivered, stored.Status);
        }

        var evt = Assert.IsType<OrderStatusChangedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("delivered", evt.Status);
    }

    [Fact]
    public async Task FromNew_ReturnsInvalidTransition()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateNew(number: 2021));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeliverOrderHandler(db, new FakePublisher());
        var result = await handler.Handle(new DeliverOrderCommand(2021), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_INVALID_TRANSITION", result.Error!.Value.Code);
    }
}
