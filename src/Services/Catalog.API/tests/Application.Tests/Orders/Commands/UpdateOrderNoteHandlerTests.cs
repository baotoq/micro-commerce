using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class UpdateOrderNoteHandlerTests
{
    [Fact]
    public async Task PersistsNote()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Orders.Add(TestOrders.CreateNew(number: 2040));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new UpdateOrderNoteHandler(act, publisher);
            var dto = await handler.Handle(new UpdateOrderNoteCommand(2040, "Held until budstem restocks Tue."), TestContext.Current.CancellationToken);
            Assert.NotNull(dto);
            Assert.Equal("Held until budstem restocks Tue.", dto!.InternalNote);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = Assert.Single(assert.Orders);
            Assert.Equal("Held until budstem restocks Tue.", stored.InternalNote);
        }

        var evt = Assert.IsType<OrderUpdatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(2040, evt.Number);
    }

    [Fact]
    public async Task UnknownNumber_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new UpdateOrderNoteHandler(db, new FakePublisher());

        var dto = await handler.Handle(new UpdateOrderNoteCommand(9999, "x"), TestContext.Current.CancellationToken);

        Assert.Null(dto);
    }
}
