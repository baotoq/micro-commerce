using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Commands;

public class UpdatePromotionHandlerTests
{
    [Fact]
    public async Task Handle_ExistingDraft_UpdatesAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20", percentValue: 20));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdatePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(
            new UpdatePromotionCommand("SPRING20", "new copy", "percentage", 30, null, 25m, null, null),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("SPRING20", result.Code);
        Assert.Equal("new copy", result.Description);
        Assert.Equal(30, result.PercentValue);
        Assert.Equal(25m, result.MinOrderAmount);
    }

    [Fact]
    public async Task Handle_ExistingDraft_PersistsChangesToDb()
    {
        // Regression for project memory: handler must use AsTracking() before mutating.
        // With the default NoTracking, SaveChanges silently no-ops.
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20", percentValue: 20));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdatePromotionHandler(db, new FakePublisher());
        await handler.Handle(
            new UpdatePromotionCommand("SPRING20", "new copy", "percentage", 30, null, null, null, null),
            TestContext.Current.CancellationToken);

        await using var verify = db;
        var stored = await verify.Promotions.FindAsync([db.Promotions.Single().Id], TestContext.Current.CancellationToken);
        Assert.NotNull(stored);
        Assert.Equal("new copy", stored.Description);
        Assert.Equal(30, stored.PercentValue);
    }

    [Fact]
    public async Task Handle_NonExistentCode_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new UpdatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(
            new UpdatePromotionCommand("MISSING", "x", "percentage", 10, null, null, null, null),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_EndedPromotion_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var promo = TestPromotions.CreatePercentage(code: "OLDPROMO");
        promo.Activate();
        promo.End();
        db.Promotions.Add(promo);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdatePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(
            new UpdatePromotionCommand("OLDPROMO", "new desc", "percentage", 30, null, null, null, null),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_Success_PublishesUpdatedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20", percentValue: 20));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new UpdatePromotionHandler(db, publisher);
        await handler.Handle(
            new UpdatePromotionCommand("SPRING20", "updated", "percentage", 25, null, null, null, null),
            TestContext.Current.CancellationToken);

        var evt = Assert.IsType<PromotionUpdatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("SPRING20", evt.Code);
    }
}
