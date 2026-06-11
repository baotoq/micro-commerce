using MicroCommerce.Catalog.Domain.Marketing;

namespace MicroCommerce.Catalog.Domain.Tests.Marketing;

public class CampaignTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7));

    private static Campaign Create(
        string name = "Spring restock",
        string subject = "Fresh clay pieces just landed",
        string previewText = "New vases, bowls, and more",
        string audienceKey = "all-subscribers",
        string templateKey = "product-highlight",
        bool followupEnabled = false,
        CampaignFollowup? followup = null,
        string? featuredProductSku = null,
        DateTimeOffset? createdAt = null) =>
        new(
            name,
            subject,
            previewText,
            audienceKey,
            templateKey,
            followupEnabled,
            followup,
            featuredProductSku,
            createdAt ?? CreatedAt);

    [Fact]
    public void Campaign_Valid_Creates()
    {
        var campaign = Create();

        Assert.NotEqual(default, campaign.Id.Value);
        Assert.Equal("Spring restock", campaign.Name);
        Assert.Equal("Fresh clay pieces just landed", campaign.Subject);
        Assert.Equal("New vases, bowls, and more", campaign.PreviewText);
        Assert.Equal("all-subscribers", campaign.AudienceKey);
        Assert.Equal("product-highlight", campaign.TemplateKey);
        Assert.Equal(CampaignStatus.Draft, campaign.Status);
        Assert.Null(campaign.ScheduledAt);
        Assert.False(campaign.FollowupEnabled);
        Assert.Null(campaign.Followup);
        Assert.Null(campaign.FeaturedProductSku);
        Assert.Equal(CreatedAt, campaign.CreatedAt);
    }

    [Fact]
    public void Schedule_FromDraft_Transitions()
    {
        var campaign = Create();
        var at = CreatedAt.AddDays(2);

        campaign.Schedule(at);

        Assert.Equal(CampaignStatus.Scheduled, campaign.Status);
        Assert.Equal(at, campaign.ScheduledAt);
    }

    [Fact]
    public void Schedule_PastDate_Throws()
    {
        var campaign = Create();
        var at = CreatedAt.AddDays(-1);

        Assert.Throws<InvalidOperationException>(() => campaign.Schedule(at));
    }

    [Fact]
    public void Send_FromScheduled_Transitions()
    {
        var campaign = Create();
        campaign.Schedule(CreatedAt.AddDays(2));

        campaign.Send();

        Assert.Equal(CampaignStatus.Sent, campaign.Status);
    }

    [Fact]
    public void Cancel_FromDraft_Transitions()
    {
        var campaign = Create();

        campaign.Cancel();

        Assert.Equal(CampaignStatus.Cancelled, campaign.Status);
    }

    [Fact]
    public void Cancel_FromSent_Throws()
    {
        var campaign = Create();
        campaign.Send();

        Assert.Throws<InvalidOperationException>(() => campaign.Cancel());
    }
}
