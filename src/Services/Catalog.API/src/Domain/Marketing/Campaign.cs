namespace MicroCommerce.Catalog.Domain.Marketing;

public class Campaign
{
    public CampaignId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string PreviewText { get; private set; } = string.Empty;
    public string AudienceKey { get; private set; } = string.Empty;
    public string TemplateKey { get; private set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; private set; }
    public CampaignStatus Status { get; private set; }
    public bool FollowupEnabled { get; private set; }
    public CampaignFollowup? Followup { get; private set; }
    public string? FeaturedProductSku { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Campaign() { }

    public Campaign(
        string name,
        string subject,
        string previewText,
        string audienceKey,
        string templateKey,
        bool followupEnabled,
        CampaignFollowup? followup,
        string? featuredProductSku,
        DateTimeOffset createdAt)
    {
        ValidateInvariants(name, subject, audienceKey, templateKey);

        Id = CampaignId.New();
        Name = name;
        Subject = subject;
        PreviewText = previewText;
        AudienceKey = audienceKey;
        TemplateKey = templateKey;
        ScheduledAt = null;
        Status = CampaignStatus.Draft;
        FollowupEnabled = followupEnabled;
        Followup = followup;
        FeaturedProductSku = featuredProductSku;
        CreatedAt = createdAt;
    }

    public void Schedule(DateTimeOffset at)
    {
        if (Status != CampaignStatus.Draft)
            throw new InvalidOperationException($"Cannot schedule campaign in status '{Status}'.");
        if (at <= CreatedAt)
            throw new InvalidOperationException("Scheduled time must be after the campaign was created.");

        Status = CampaignStatus.Scheduled;
        ScheduledAt = at;
    }

    public void Send()
    {
        if (Status is not (CampaignStatus.Scheduled or CampaignStatus.Draft))
            throw new InvalidOperationException($"Cannot send campaign in status '{Status}'.");

        Status = CampaignStatus.Sent;
    }

    public void Cancel()
    {
        if (Status is not (CampaignStatus.Draft or CampaignStatus.Scheduled))
            throw new InvalidOperationException($"Cannot cancel campaign in status '{Status}'.");

        Status = CampaignStatus.Cancelled;
    }

    private static void ValidateInvariants(
        string name,
        string subject,
        string audienceKey,
        string templateKey)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(audienceKey))
            throw new ArgumentException("AudienceKey required.", nameof(audienceKey));
        if (string.IsNullOrWhiteSpace(templateKey))
            throw new ArgumentException("TemplateKey required.", nameof(templateKey));
    }
}
