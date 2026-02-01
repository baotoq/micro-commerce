using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Entities;

/// <summary>
/// Represents a stock reservation that temporarily holds quantity.
/// Owned by StockItem aggregate - not independently accessible.
/// Reservations have a TTL and expire automatically.
/// </summary>
public sealed class StockReservation
{
    public ReservationId Id { get; private set; } = null!;
    public StockItemId StockItemId { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

    // EF Core constructor
    private StockReservation()
    {
    }

    public static StockReservation Create(StockItemId stockItemId, int quantity, TimeSpan ttl)
    {
        return new StockReservation
        {
            Id = ReservationId.New(),
            StockItemId = stockItemId,
            Quantity = quantity,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl)
        };
    }
}
