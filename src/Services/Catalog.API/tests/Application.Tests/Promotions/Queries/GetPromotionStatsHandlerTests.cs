using MicroCommerce.Catalog.Application.Promotions.Queries;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Queries;

public class GetPromotionStatsHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDb_ReturnsAllZeros()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetPromotionStatsHandler(db);

        var result = await handler.Handle(new GetPromotionStatsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(new(0, 0, 0, 0), result);
    }

    [Fact]
    public async Task Handle_Mixed_ReturnsCorrectCounts()
    {
        await using var db = DbContextFactory.Create();
        var active1 = TestPromotions.CreatePercentage(code: "A1"); active1.Activate();
        var active2 = TestPromotions.CreatePercentage(code: "A2"); active2.Activate();
        var draft = TestPromotions.CreatePercentage(code: "D1");
        var ended = TestPromotions.CreatePercentage(code: "E1"); ended.Activate(); ended.End();

        db.Promotions.AddRange(active1, active2, draft, ended);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionStatsHandler(db);
        var result = await handler.Handle(new GetPromotionStatsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Active);
        Assert.Equal(1, result.Draft);
        Assert.Equal(1, result.Ended);
        Assert.Equal(4, result.Total);
    }
}
