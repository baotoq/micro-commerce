namespace MicroCommerce.ApiService.Features.Ordering.Application.Saga;

/// <summary>
/// All saga message contracts for the checkout state machine.
/// Every contract has an OrderId for saga correlation.
/// </summary>

/// <summary>
/// Published by SubmitOrderCommandHandler to initiate the checkout saga.
/// </summary>
public record CheckoutStarted
{
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerEmail { get; init; } = null!;
    public List<CheckoutItem> Items { get; init; } = [];
}

/// <summary>
/// Product and quantity for stock reservation.
/// </summary>
public record CheckoutItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

/// <summary>
/// Saga -> ReserveStockForOrderConsumer: reserve stock for each item.
/// </summary>
public record ReserveStockForOrder
{
    public Guid OrderId { get; init; }
    public List<CheckoutItem> Items { get; init; } = [];
}

/// <summary>
/// ReserveStockForOrderConsumer -> Saga: all reservations succeeded.
/// ReservationIdsJson is a serialized Dictionary&lt;Guid, Guid&gt; (ProductId -> ReservationId).
/// </summary>
public record StockReservationCompleted
{
    public Guid OrderId { get; init; }
    public string ReservationIdsJson { get; init; } = null!;
}

/// <summary>
/// ReserveStockForOrderConsumer -> Saga: stock reservation failed for at least one item.
/// </summary>
public record StockReservationFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Published by SimulatePaymentCommandHandler when payment succeeds.
/// </summary>
public record PaymentCompleted
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Published by SimulatePaymentCommandHandler when payment fails.
/// </summary>
public record PaymentFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Saga -> ConfirmOrderConsumer: set order status to Confirmed.
/// </summary>
public record ConfirmOrder
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Saga -> OrderFailedConsumer: set order status to Failed.
/// </summary>
public record OrderFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Saga -> DeductStockConsumer: permanently deduct stock for confirmed order.
/// ReservationIdsJson is a serialized Dictionary&lt;Guid, Guid&gt; (ProductId -> ReservationId).
/// </summary>
public record DeductStock
{
    public Guid OrderId { get; init; }
    public string ReservationIdsJson { get; init; } = null!;
}

/// <summary>
/// Saga -> ReleaseStockReservationsConsumer: compensation - release all reservations on failure.
/// ReservationIdsJson is a serialized Dictionary&lt;Guid, Guid&gt; (ProductId -> ReservationId).
/// </summary>
public record ReleaseStockReservations
{
    public Guid OrderId { get; init; }
    public string ReservationIdsJson { get; init; } = null!;
}

/// <summary>
/// Saga -> ClearCartConsumer: delete the buyer's cart after successful checkout.
/// </summary>
public record ClearCart
{
    public Guid BuyerId { get; init; }
}
