using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Domain.Marketing;

namespace MicroCommerce.Catalog.Application.Marketing;

internal static class MarketingMapping
{
    public static readonly IReadOnlyList<string> AudienceOptions =
    [
        "all-subscribers",
        "past-buyers",
        "vip",
    ];

    public static readonly IReadOnlyList<string> TemplateOptions =
    [
        "product-highlight",
        "newsletter",
        "reengagement",
    ];

    public static CampaignDto ToDto(Campaign c) =>
        new(
            c.Id.Value,
            c.Name,
            c.Subject,
            c.PreviewText,
            c.AudienceKey,
            c.TemplateKey,
            c.ScheduledAt,
            StatusToString(c.Status),
            c.FollowupEnabled,
            c.Followup is null ? null : ToFollowupDto(c.Followup),
            c.FeaturedProductSku,
            c.CreatedAt);

    public static CampaignFollowupDto ToFollowupDto(CampaignFollowup f) =>
        new(f.Enabled, KindToString(f.Kind), f.DelayDays, f.Subject, f.Preview);

    public static string StatusToString(CampaignStatus status) => status switch
    {
        CampaignStatus.Draft => "draft",
        CampaignStatus.Scheduled => "scheduled",
        CampaignStatus.Sent => "sent",
        CampaignStatus.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown campaign status."),
    };

    public static CampaignStatus? TryParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "draft" => CampaignStatus.Draft,
        "scheduled" => CampaignStatus.Scheduled,
        "sent" => CampaignStatus.Sent,
        "cancelled" => CampaignStatus.Cancelled,
        _ => null,
    };

    public static string KindToString(FollowupKind kind) => kind switch
    {
        FollowupKind.ViewedNotPurchased => "ViewedNotPurchased",
        FollowupKind.PurchasedHighValue => "PurchasedHighValue",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown followup kind."),
    };

    public static FollowupKind ParseKind(string s) => s switch
    {
        "ViewedNotPurchased" => FollowupKind.ViewedNotPurchased,
        "PurchasedHighValue" => FollowupKind.PurchasedHighValue,
        _ => throw new ArgumentException($"Invalid followup kind: {s}"),
    };
}
