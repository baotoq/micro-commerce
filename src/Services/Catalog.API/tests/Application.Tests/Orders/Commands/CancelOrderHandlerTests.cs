using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class CancelOrderHandlerTests
{
    [Fact]
    public async Task FromNew_PersistsCancelled()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Orders.Add(TestOrders.CreateNew(number: 2030));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new CancelOrderHandler(act, publisher);
            var result = await handler.Handle(new CancelOrderCommand(2030), TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess);
            Assert.Equal("cancelled", result.Value!.Status);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = Assert.Single(assert.Orders);
            Assert.Equal(OrderStatus.Cancelled, stored.Status);
        }

        var evt = Assert.IsType<OrderStatusChangedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("cancelled", evt.Status);
    }

    [Fact]
    public async Task FromDelivered_ReturnsInvalidTransition()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateDelivered(number: 2031));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CancelOrderHandler(db, new FakePublisher());
        var result = await handler.Handle(new CancelOrderCommand(2031), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_INVALID_TRANSITION", result.Error!.Value.Code);
    }
}
