using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Commands;

public class ActivatePromotionHandlerTests
{
    [Fact]
    public async Task Handle_DraftPromotion_Activates()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ActivatePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(new ActivatePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("active", result.Value!.Status);
        Assert.Equal(PromotionStatus.Active, db.Promotions.Single().Status);
    }

    [Fact]
    public async Task Handle_Missing_ReturnsNotFoundConflict()
    {
        await using var db = DbContextFactory.Create();
        var handler = new ActivatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(new ActivatePromotionCommand("MISSING"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMOTION_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Handle_AlreadyActive_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var promo = TestPromotions.CreatePercentage(code: "SPRING20");
        promo.Activate();
        db.Promotions.Add(promo);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ActivatePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(new ActivatePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMOTION_INVALID_TRANSITION", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Handle_Ended_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var promo = TestPromotions.CreatePercentage(code: "SPRING20");
        promo.Activate();
        promo.End();
        db.Promotions.Add(promo);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ActivatePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(new ActivatePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMOTION_INVALID_TRANSITION", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Handle_Success_PublishesStatusChangedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new ActivatePromotionHandler(db, publisher);
        await handler.Handle(new ActivatePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<PromotionStatusChangedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("SPRING20", evt.Code);
        Assert.Equal("active", evt.Status);
    }
}
