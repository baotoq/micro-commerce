using MicroCommerce.Catalog.Application.Orders.Queries;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Queries;

public class GetOrderCountsHandlerTests
{
    [Fact]
    public async Task CountsAllStatuses_Correctly()
    {
        await using var db = DbContextFactory.Create();
        // 2 New, 1 Packed, 1 Shipped, 1 Delivered, 1 RefundRequested, 1 Cancelled.
        db.Orders.Add(TestOrders.CreateNew(number: 1042));
        db.Orders.Add(TestOrders.CreateNew(number: 1041, lines: [TestOrders.Line(unitPrice: 64.00m)]));
        db.Orders.Add(TestOrders.CreatePacked(number: 1039));
        db.Orders.Add(TestOrders.CreateShipped(number: 1038));
        db.Orders.Add(TestOrders.CreateDelivered(number: 1035));
        db.Orders.Add(TestOrders.CreateRefundRequested(number: 1036));
        db.Orders.Add(TestOrders.CreateCancelled(number: 1033));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrderCountsHandler(db);
        var result = await handler.Handle(new GetOrderCountsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(7, result.All);
        Assert.Equal(2, result.New);
        Assert.Equal(1, result.Packed);
        Assert.Equal(1, result.Shipped);
        Assert.Equal(1, result.Delivered);
        Assert.Equal(3, result.NeedsAction); // 2 New + 1 RefundRequested
        Assert.Equal(2, result.RefundOrCancel); // 1 RefundRequested + 1 Cancelled
        // LifetimeRevenue excludes the Cancelled order (default Paid = 86 each line).
        Assert.True(result.LifetimeRevenue > 0m);
    }

    [Fact]
    public async Task EmptyDatabase_ReturnsZeros()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetOrderCountsHandler(db);

        var result = await handler.Handle(new GetOrderCountsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(0, result.All);
        Assert.Equal(0m, result.LifetimeRevenue);
    }
}
