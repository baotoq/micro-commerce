namespace MicroCommerce.Catalog.Application.Marketing.Dtos;

public record CampaignFollowupDto(
    bool Enabled,
    string Kind,
    int DelayDays,
    string Subject,
    string Preview);

public record CampaignDto(
    Guid Id,
    string Name,
    string Subject,
    string PreviewText,
    string AudienceKey,
    string TemplateKey,
    DateTimeOffset? ScheduledAt,
    string Status,
    bool FollowupEnabled,
    CampaignFollowupDto? Followup,
    string? FeaturedProductSku,
    DateTimeOffset CreatedAt);

public record MarketingDraftDto(
    Guid Id,
    string Subject,
    int SubjectCharCount,
    string AudienceKey,
    string TemplateKey,
    string? ProductSku,
    string? ProductName,
    string? ProductInventoryLabel,
    IReadOnlyList<string> AudienceOptions,
    IReadOnlyList<string> TemplateOptions);
