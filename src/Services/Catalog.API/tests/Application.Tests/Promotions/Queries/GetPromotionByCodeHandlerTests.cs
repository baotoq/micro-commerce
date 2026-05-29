using MicroCommerce.Catalog.Application.Promotions.Queries;

namespace MicroCommerce.Catalog.Application.Tests.Promotions.Queries;

public class GetPromotionByCodeHandlerTests
{
    [Fact]
    public async Task Handle_Existing_ReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPromotionByCodeHandler(db);
        var result = await handler.Handle(new GetPromotionByCodeQuery("SPRING20"), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("SPRING20", result.Code);
        Assert.Equal("percentage", result.Kind);
    }

    [Fact]
    public async Task Handle_Missing_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetPromotionByCodeHandler(db);

        var result = await handler.Handle(new GetPromotionByCodeQuery("MISSING"), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsHandle_Existing_ReturnsTrue()
    {
        await using var db = DbContextFactory.Create();
        db.Promotions.Add(TestPromotions.CreatePercentage(code: "SPRING20"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new PromotionExistsByCodeHandler(db);
        var result = await handler.Handle(new PromotionExistsByCodeQuery("SPRING20"), TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsHandle_Missing_ReturnsFalse()
    {
        await using var db = DbContextFactory.Create();
        var handler = new PromotionExistsByCodeHandler(db);

        var result = await handler.Handle(new PromotionExistsByCodeQuery("MISSING"), TestContext.Current.CancellationToken);

        Assert.False(result);
    }
}
