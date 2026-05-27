using MicroCommerce.Catalog.Application.Products.Commands;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class CreateProductRichFieldsTests
{
    private static CreateProductCommand ValidCommand(string sku = "MC-RT-001") => new(
        Sku: sku,
        Name: "Persimmon vase",
        Category: "Vessels",
        Price: 86m,
        Inventory: 24,
        Status: "active",
        Description: "A tall hand-thrown vase.",
        Tags: ["ceramic", "minimal"],
        Weight: 1.25m,
        Origin: "Portland, OR",
        PhotoUrls: ["https://example.com/p1.jpg"]);

    [Fact]
    public async Task Handle_PersistsAllRichFields()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("A tall hand-thrown vase.", dto.Description);
        Assert.Equal(new[] { "ceramic", "minimal" }, dto.Tags);
        Assert.Equal(1.25m, dto.Weight);
        Assert.Equal("Portland, OR", dto.Origin);
        Assert.Equal(new[] { "https://example.com/p1.jpg" }, dto.PhotoUrls);
    }

    [Fact]
    public async Task Handle_DraftStatus_AcceptsEmptyPhotoUrls()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var cmd = ValidCommand() with { Status = "draft", PhotoUrls = [] };
        var result = await handler.Handle(cmd, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.PhotoUrls);
    }

    [Fact]
    public async Task Handle_DescriptionNull_IsAccepted()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var cmd = ValidCommand() with { Description = null };
        var result = await handler.Handle(cmd, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Description);
    }

    [Fact]
    public async Task Handle_TagsDeduplicated()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var cmd = ValidCommand() with { Tags = ["a", "a", "b"] };
        var result = await handler.Handle(cmd, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "a", "b" }, result.Value!.Tags);
    }
}
