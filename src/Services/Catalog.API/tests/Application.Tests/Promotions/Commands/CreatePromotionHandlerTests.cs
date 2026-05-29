using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Commands;

public class CreatePromotionHandlerTests
{
    private static CreatePromotionCommand PercentageCmd(string code = "SPRING20", bool activateImmediately = false) =>
        new(code, "sitewide", "percentage", 20, null, null, null, null, activateImmediately);

    [Fact]
    public async Task Handle_ValidPercentage_CreatesPromotionAsDraft()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(PercentageCmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("SPRING20", result.Value!.Code);
        Assert.Equal("percentage", result.Value.Kind);
        Assert.Equal(20, result.Value.PercentValue);
        Assert.Equal("draft", result.Value.Status);
        Assert.Single(db.Promotions);
    }

    [Fact]
    public async Task Handle_ValidFixed_CreatesPromotion()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(
            new CreatePromotionCommand("WELCOME10", "first order", "fixed", null, 10m, 40m, null, null, false),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("WELCOME10", result.Value!.Code);
        Assert.Equal("fixed", result.Value.Kind);
        Assert.Equal(10m, result.Value.FixedAmount);
        Assert.Equal(40m, result.Value.MinOrderAmount);
    }

    [Fact]
    public async Task Handle_ActivateImmediately_PersistsAsActiveInSingleSaveChanges()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(PercentageCmd(activateImmediately: true), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("active", result.Value!.Status);
        var stored = db.Promotions.Single();
        Assert.Equal(PromotionStatus.Active, stored.Status);
    }

    [Fact]
    public async Task Handle_CodeIsTrimmedAndUppercased()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreatePromotionHandler(db, new FakePublisher());

        var result = await handler.Handle(
            new CreatePromotionCommand("  spring20  ", "sitewide", "percentage", 20, null, null, null, null, false),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("SPRING20", result.Value!.Code);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreatePromotionHandler(db, publisher);
        await handler.Handle(PercentageCmd(), TestContext.Current.CancellationToken);
        publisher.Published.Clear();

        var result = await handler.Handle(PercentageCmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMOTION_CODE_DUPLICATE", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task Handle_Success_PublishesCreatedEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreatePromotionHandler(db, publisher);

        await handler.Handle(PercentageCmd(), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<PromotionCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("SPRING20", evt.Code);
        Assert.Equal("draft", evt.Status);
    }

    [Fact]
    public async Task Handle_ActivateImmediatelyAndSuccess_PublishesCreatedEventWithActiveStatus()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreatePromotionHandler(db, publisher);

        await handler.Handle(PercentageCmd(activateImmediately: true), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<PromotionCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("active", evt.Status);
    }

    [Fact]
    public async Task Handle_InvalidKind_ThrowsArgumentException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreatePromotionHandler(db, new FakePublisher());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreatePromotionCommand("CODE1", "x", "bogus", 10, null, null, null, null, false),
            TestContext.Current.CancellationToken));
    }
}
