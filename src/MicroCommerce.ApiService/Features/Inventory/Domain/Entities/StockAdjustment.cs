using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Entities;

/// <summary>
/// Audit record for stock adjustments.
/// Separate from StockItem aggregate - persisted independently for audit trail.
/// </summary>
public sealed class StockAdjustment
{
    public AdjustmentId Id { get; private set; } = null!;
    public StockItemId StockItemId { get; private set; } = null!;
    public int Adjustment { get; private set; }
    public int QuantityAfter { get; private set; }
    public string? Reason { get; private set; }
    public string? AdjustedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // EF Core constructor
    private StockAdjustment()
    {
    }

    public static StockAdjustment Create(
        StockItemId stockItemId,
        int adjustment,
        int quantityAfter,
        string? reason,
        string? adjustedBy)
    {
        return new StockAdjustment
        {
            Id = AdjustmentId.New(),
            StockItemId = stockItemId,
            Adjustment = adjustment,
            QuantityAfter = quantityAfter,
            Reason = reason,
            AdjustedBy = adjustedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
