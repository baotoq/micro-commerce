using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Events;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Commands;

public class DeletePromotionHandlerTests
{
    [Fact]
    public async Task Handle_Existing_RemovesAndReturnsTrue()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeletePromotionHandler(db, new FakePublisher());
        var result = await handler.Handle(new DeletePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Empty(db.Promotions);
    }

    [Fact]
    public async Task Handle_Missing_ReturnsFalse()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeletePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(new DeletePromotionCommand("MISSING"), TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_Success_PublishesDeletedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new DeletePromotionHandler(db, publisher);
        await handler.Handle(new DeletePromotionCommand("SPRING20"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<PromotionDeletedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("SPRING20", evt.Code);
    }

    [Fact]
    public async Task Handle_Missing_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new DeletePromotionHandler(db, publisher);

        await handler.Handle(new DeletePromotionCommand("MISSING"), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
