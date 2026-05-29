using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Promotions;

internal static class PromotionMapping
{
    public static PromotionDto ToDto(Promotion p) =>
        new(
            p.Code.Value,
            p.Description,
            KindToString(p.Kind),
            p.PercentValue,
            p.FixedAmount,
            p.MinOrderAmount,
            p.StartsAt,
            p.EndsAt,
            StatusToString(p.Status));

    public static string KindToString(DiscountKind kind) => kind switch
    {
        DiscountKind.Percentage => "percentage",
        DiscountKind.FixedAmount => "fixed",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown discount kind."),
    };

    public static DiscountKind ParseKind(string s) => s.ToLowerInvariant() switch
    {
        "percentage" => DiscountKind.Percentage,
        "fixed" => DiscountKind.FixedAmount,
        _ => throw new ArgumentException($"Invalid discount kind: {s}"),
    };

    public static string StatusToString(PromotionStatus status) => status switch
    {
        PromotionStatus.Draft => "draft",
        PromotionStatus.Active => "active",
        PromotionStatus.Ended => "ended",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown promotion status."),
    };

    public static PromotionStatus? TryParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "draft" => PromotionStatus.Draft,
        "active" => PromotionStatus.Active,
        "ended" => PromotionStatus.Ended,
        _ => null,
    };
}
