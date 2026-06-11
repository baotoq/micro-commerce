using MicroCommerce.Catalog.Application.Orders.Queries;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Queries;

public class GetOrdersHandlerTests
{
    [Fact]
    public async Task TabNeedsAction_ReturnsNewAndRefundRequested()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateNew(number: 1042));
        db.Orders.Add(TestOrders.CreatePacked(number: 1039));
        db.Orders.Add(TestOrders.CreateShipped(number: 1038));
        db.Orders.Add(TestOrders.CreateRefundRequested(number: 1036));
        db.Orders.Add(TestOrders.CreateCancelled(number: 1033));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrdersHandler(db);
        var result = await handler.Handle(new GetOrdersQuery(1, 50, "needs-action"), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Total);
        var numbers = result.Items.Select(i => i.Number).OrderBy(n => n).ToList();
        Assert.Equal([1036, 1042], numbers);
    }

    [Fact]
    public async Task TabAll_ReturnsSortedByPlacedAtDesc()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateNew(number: 1040, placedAt: new DateTimeOffset(2026, 4, 8, 9, 41, 0, TimeSpan.FromHours(-7))));
        db.Orders.Add(TestOrders.CreateNew(number: 1042, placedAt: new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))));
        db.Orders.Add(TestOrders.CreateNew(number: 1041, placedAt: new DateTimeOffset(2026, 4, 8, 11, 8, 0, TimeSpan.FromHours(-7))));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrdersHandler(db);
        var result = await handler.Handle(new GetOrdersQuery(1, 50, "all"), TestContext.Current.CancellationToken);

        Assert.Equal([1042, 1041, 1040], result.Items.Select(i => i.Number).ToList());
    }

    [Fact]
    public async Task Pagination_Works()
    {
        await using var db = DbContextFactory.Create();
        for (var i = 0; i < 5; i++)
            db.Orders.Add(TestOrders.CreateNew(
                number: 1000 + i,
                placedAt: new DateTimeOffset(2026, 4, 8, 8, 0, 0, TimeSpan.FromHours(-7)).AddMinutes(i)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrdersHandler(db);
        var result = await handler.Handle(new GetOrdersQuery(2, 2, "all"), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }
}
