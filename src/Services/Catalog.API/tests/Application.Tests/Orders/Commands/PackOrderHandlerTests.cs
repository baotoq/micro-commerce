using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class PackOrderHandlerTests
{
    [Fact]
    public async Task FromNew_PersistsPacked_AndPublishesStatusChangedEvent()
    {
        var dbName = Guid.NewGuid().ToString();

        // Context 1: seed a New order.
        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Orders.Add(TestOrders.CreateNew(number: 2001));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Context 2: run the handler over the SAME db name. Without .AsTracking() this
        // SaveChanges is a silent no-op (project_ef_notracking_mutating_handlers).
        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new PackOrderHandler(act, publisher);
            var result = await handler.Handle(new PackOrderCommand(2001), TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess);
            Assert.Equal("packed", result.Value!.Status);
        }

        // Context 3: fresh read proves the mutation persisted.
        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = Assert.Single(assert.Orders);
            Assert.Equal(OrderStatus.Packed, stored.Status);
        }

        var evt = Assert.IsType<OrderStatusChangedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(2001, evt.Number);
        Assert.Equal("packed", evt.Status);
    }

    [Fact]
    public async Task UnknownNumber_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var handler = new PackOrderHandler(db, new FakePublisher());

        var result = await handler.Handle(new PackOrderCommand(9999), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task FromShipped_ReturnsInvalidTransition()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateShipped(number: 2002));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new PackOrderHandler(db, new FakePublisher());
        var result = await handler.Handle(new PackOrderCommand(2002), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_INVALID_TRANSITION", result.Error!.Value.Code);
    }
}
