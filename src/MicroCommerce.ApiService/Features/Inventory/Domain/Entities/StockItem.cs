using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Inventory.Domain.Events;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Entities;

/// <summary>
/// StockItem aggregate root for the inventory domain.
/// Tracks quantity on hand, manages reservations, and enforces stock invariants.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class StockItem : BaseAggregateRoot<StockItemId>
{
    private static readonly TimeSpan DefaultReservationTtl = TimeSpan.FromMinutes(15);

    private readonly List<StockReservation> _reservations = [];

    /// <summary>
    /// ProductId as raw Guid - cross-module boundary, NOT a navigation property.
    /// </summary>
    public Guid ProductId { get; private set; }

    public int QuantityOnHand { get; private set; }

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    /// <summary>
    /// Available quantity accounting for active (non-expired) reservations.
    /// </summary>
    public int AvailableQuantity =>
        QuantityOnHand - _reservations
            .Where(r => r.ExpiresAt > DateTimeOffset.UtcNow)
            .Sum(r => r.Quantity);

    // EF Core constructor
    private StockItem(StockItemId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new stock item.
    /// No domain event on creation - stock items are created by consumers, not user action.
    /// </summary>
    public static StockItem Create(Guid productId)
    {
        return new StockItem(StockItemId.New())
        {
            ProductId = productId,
            QuantityOnHand = 0
        };
    }

    /// <summary>
    /// Adjusts the stock quantity (positive for restock, negative for removal).
    /// Enforces non-negative stock invariant.
    /// Raises StockAdjustedDomainEvent and optionally StockLowDomainEvent.
    /// </summary>
    public void AdjustStock(int adjustment, string? reason = null, string? adjustedBy = null)
    {
        var newQuantity = QuantityOnHand + adjustment;

        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Stock adjustment would result in negative quantity. Current: {QuantityOnHand}, Adjustment: {adjustment}");

        QuantityOnHand = newQuantity;

        AddDomainEvent(new StockAdjustedDomainEvent(Id.Value, ProductId, adjustment, newQuantity));

        if (newQuantity <= 10)
        {
            AddDomainEvent(new StockLowDomainEvent(Id.Value, ProductId, newQuantity));
        }
    }

    /// <summary>
    /// Reserves a quantity of stock for a pending order.
    /// Validates sufficient available quantity and creates a time-limited reservation.
    /// </summary>
    public ReservationId Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be positive.", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException(
                $"Insufficient available stock. Available: {AvailableQuantity}, Requested: {quantity}");

        var reservation = StockReservation.Create(Id, quantity, DefaultReservationTtl);
        _reservations.Add(reservation);

        AddDomainEvent(new StockReservedDomainEvent(Id.Value, ProductId, reservation.Id.Value, quantity));

        return reservation.Id;
    }

    /// <summary>
    /// Releases a reservation, making the quantity available again.
    /// No-op if reservation not found (idempotent).
    /// </summary>
    public void ReleaseReservation(ReservationId reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return;

        _reservations.Remove(reservation);

        AddDomainEvent(new StockReleasedDomainEvent(Id.Value, ProductId, reservationId.Value, reservation.Quantity));
    }
}
