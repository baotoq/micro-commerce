namespace MicroCommerce.Catalog.Application.Promotions.Dtos;

public record PromotionDto(
    string Code,
    string Description,
    string Kind,
    int? PercentValue,
    decimal? FixedAmount,
    decimal? MinOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string Status);

public record PromotionStatsDto(int Active, int Draft, int Ended, int Total);
