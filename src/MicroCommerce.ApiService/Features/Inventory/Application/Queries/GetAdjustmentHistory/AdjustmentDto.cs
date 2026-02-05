namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetAdjustmentHistory;

public sealed record AdjustmentDto(
    Guid Id,
    int Adjustment,
    int QuantityAfter,
    string? Reason,
    string? AdjustedBy,
    DateTimeOffset CreatedAt);
