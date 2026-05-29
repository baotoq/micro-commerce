using MicroCommerce.Catalog.Application.Promotions.Queries;
using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Queries;

public class GetPromotionsHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyPage()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetPromotionsHandler(db);

        var result = await handler.Handle(new GetPromotionsQuery(1, 10, null), TestContext.Current.CancellationToken);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task Handle_NoFilter_ReturnsAll()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.AddRange(
            TestPromotions.CreatePercentage(code: "ONE"),
            TestPromotions.CreatePercentage(code: "TWO"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionsHandler(db);
        var result = await handler.Handle(new GetPromotionsQuery(1, 10, null), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_StatusFilter_ReturnsMatching()
    {
        await using var db = DbContextFactory.Create();
        var p1 = TestPromotions.CreatePercentage(code: "ONE"); p1.Activate();
        var p2 = TestPromotions.CreatePercentage(code: "TWO");
        db.Promotions.AddRange(p1, p2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionsHandler(db);
        var result = await handler.Handle(new GetPromotionsQuery(1, 10, "active"), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Total);
        Assert.Equal("ONE", result.Items.Single().Code);
        Assert.Equal("active", result.Items.Single().Status);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await using var db = DbContextFactory.Create();
        for (int i = 1; i <= 5; i++) db.Promotions.Add(TestPromotions.CreatePercentage(code: $"P{i:D2}"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionsHandler(db);
        var result = await handler.Handle(new GetPromotionsQuery(2, 2, null), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task Handle_UnknownStatusFilter_ReturnsAll()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "ONE"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionsHandler(db);
        var result = await handler.Handle(new GetPromotionsQuery(1, 10, "bogus"), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Total);
    }
}
