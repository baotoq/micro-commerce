using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;
using System.Text.Json.Serialization;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

[JsonConverter(typeof(SmartEnumNameConverter<OrderStatus, int>))]
public abstract class OrderStatus : SmartEnum<OrderStatus>
{
    // Happy path states
    public static readonly OrderStatus Submitted     = new SubmittedStatus();
    public static readonly OrderStatus StockReserved = new StockReservedStatus();
    public static readonly OrderStatus Paid          = new PaidStatus();
    public static readonly OrderStatus Confirmed     = new ConfirmedStatus();
    public static readonly OrderStatus Shipped       = new ShippedStatus();
    public static readonly OrderStatus Delivered     = new DeliveredStatus();
    // Terminal non-happy states
    public static readonly OrderStatus Failed        = new FailedStatus();
    public static readonly OrderStatus Cancelled     = new CancelledStatus();

    private OrderStatus(string name, int value) : base(name, value) { }

    public abstract bool CanTransitionTo(OrderStatus next);

    public void TransitionTo(OrderStatus next)
    {
        if (!CanTransitionTo(next))
        {
            IEnumerable<string> validNames = List.Where(s => CanTransitionTo(s)).Select(s => s.Name);
            throw new InvalidOperationException(
                $"Cannot transition from '{Name}' to '{next.Name}'. " +
                $"Valid transitions from '{Name}': {string.Join(", ", validNames)}.");
        }
    }

    private sealed class SubmittedStatus : OrderStatus
    {
        public SubmittedStatus() : base("Submitted", 0) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == StockReserved || next == Paid || next == Failed || next == Cancelled;
    }

    private sealed class StockReservedStatus : OrderStatus
    {
        public StockReservedStatus() : base("StockReserved", 1) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == Paid || next == Failed || next == Cancelled;
    }

    private sealed class PaidStatus : OrderStatus
    {
        public PaidStatus() : base("Paid", 2) { }
        public override bool CanTransitionTo(OrderStatus next) =>
            next == Confirmed || next == Cancelled;
    }

    private sealed class ConfirmedStatus : OrderStatus
    {
        public ConfirmedStatus() : base("Confirmed", 3) { }
        public override bool CanTransitionTo(OrderStatus next) => next == Shipped;
    }

    private sealed class ShippedStatus : OrderStatus
    {
        public ShippedStatus() : base("Shipped", 4) { }
        public override bool CanTransitionTo(OrderStatus next) => next == Delivered;
    }

    private sealed class DeliveredStatus : OrderStatus
    {
        public DeliveredStatus() : base("Delivered", 5) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }

    private sealed class FailedStatus : OrderStatus
    {
        public FailedStatus() : base("Failed", 6) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }

    private sealed class CancelledStatus : OrderStatus
    {
        public CancelledStatus() : base("Cancelled", 7) { }
        public override bool CanTransitionTo(OrderStatus next) => false; // terminal
    }
}
