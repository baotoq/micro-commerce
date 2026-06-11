using MicroCommerce.Catalog.Application.Marketing.Commands;
using MicroCommerce.Catalog.Application.Marketing.Events;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Marketing;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.Marketing.Commands;

public class CreateCampaignHandlerTests
{
    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset Now { get; } = now;
    }

    // 46-char subject (true Subject.Length == 46); see GetMarketingDraftHandlerTests note.
    private const string Subject46 = "New arrivals for spring, handmade just for you";

    private static CreateCampaignCommand ValidCmd(IClock clock) =>
        new(
            "Spring Launch",
            Subject46,
            "Fresh from the studio.",
            "past-buyers",
            "product-highlight",
            FeaturedProductSku: "MC-VS-001",
            FollowupEnabled: true,
            Followup: new CampaignFollowupInput(true, "ViewedNotPurchased", 3, "Still thinking about it?", "Your spring picks are still here."),
            Clock: clock);

    [Fact]
    public async Task Handle_Valid_PersistsAndPublishesEvent()
    {
        var dbName = Guid.NewGuid().ToString();
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7)));
        var publisher = new FakePublisher();

        Guid id;
        await using (var db = DbContextFactory.Create(dbName))
        {
            var handler = new CreateCampaignHandler(db, publisher);
            var result = await handler.Handle(ValidCmd(clock), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Spring Launch", result.Value!.Name);
            Assert.Equal("draft", result.Value.Status);
            id = result.Value.Id;
        }

        var evt = Assert.IsType<CampaignCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(id, evt.Id);

        await using (var verify = DbContextFactory.Create(dbName))
        {
            var stored = await verify.Campaigns.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(id, stored.Id.Value);
            Assert.Equal(CampaignStatus.Draft, stored.Status);
            Assert.Equal("MC-VS-001", stored.FeaturedProductSku);
        }
    }

    [Fact]
    public async Task Handle_SetsCreatedAtFromClock()
    {
        var dbName = Guid.NewGuid().ToString();
        var anchor = new DateTimeOffset(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7));
        var clock = new FixedClock(anchor);

        Guid id;
        await using (var db = DbContextFactory.Create(dbName))
        {
            var handler = new CreateCampaignHandler(db, new FakePublisher());
            var result = await handler.Handle(ValidCmd(clock), TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess);
            Assert.Equal(anchor, result.Value!.CreatedAt);
            id = result.Value.Id;
        }

        await using (var verify = DbContextFactory.Create(dbName))
        {
            var stored = await verify.Campaigns.AsNoTracking().SingleAsync(c => c.Id == MicroCommerce.Catalog.Domain.Marketing.CampaignId.From(id), TestContext.Current.CancellationToken);
            Assert.Equal(anchor, stored.CreatedAt);
        }
    }
}
