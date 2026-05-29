using MicroCommerce.Catalog.Domain.Promotions;

namespace MicroCommerce.Catalog.Application.Tests.Promotions;

internal static class TestPromotions
{
    public static Promotion CreatePercentage(
        string code = "SPRING20",
        string description = "sitewide",
        int percentValue = 20,
        decimal? minOrderAmount = null,
        DateTimeOffset? startsAt = null,
        DateTimeOffset? endsAt = null) =>
        new(
            PromotionCode.From(code),
            description,
            DiscountKind.Percentage,
            percentValue,
            fixedAmount: null,
            minOrderAmount,
            startsAt,
            endsAt);

    public static Promotion CreateFixed(
        string code = "WELCOME10",
        string description = "first order",
        decimal fixedAmount = 10m,
        decimal? minOrderAmount = null) =>
        new(
            PromotionCode.From(code),
            description,
            DiscountKind.FixedAmount,
            percentValue: null,
            fixedAmount,
            minOrderAmount,
            startsAt: null,
            endsAt: null);
}
