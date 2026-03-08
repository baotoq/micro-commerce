using MicroCommerce.ApiService.Features.Ordering.Domain.Events;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Entities;

/// <summary>
/// Order aggregate root for the ordering domain.
/// Manages order items, enforces invariants, and tracks order lifecycle.
/// </summary>
public sealed class Order : BaseAggregateRoot<OrderId>, IConcurrencyToken
{
    private const decimal FlatShippingCost = 5.99m;
    private const decimal TaxRate = 0.08m;

    private readonly List<OrderItem> _items = [];

    public OrderNumber OrderNumber { get; private set; } = null!;
    public Guid BuyerId { get; private set; }
    public string BuyerEmail { get; private set; } = null!;
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public string? CouponCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    public int Version { get; set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Order(OrderId id) : base(id)
    {
        Status = OrderStatus.Submitted;
    }

    /// <summary>
    /// Factory method for creating a new order from cart items.
    /// Calculates subtotal, shipping, tax, and total automatically.
    /// </summary>
    public static Order Create(
        Guid buyerId,
        string buyerEmail,
        ShippingAddress address,
        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items,
        string? couponCode = null,
        decimal discountAmount = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buyerEmail);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(items);

        OrderId orderId = OrderId.New();
        Order order = new(orderId)
        {
            OrderNumber = OrderNumber.Generate(),
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            ShippingAddress = address,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTimeOffset.UtcNow,
            CouponCode = couponCode
        };

        foreach ((Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity) item in items)
        {
            OrderItem orderItem = OrderItem.Create(
                orderId,
                item.productId,
                item.productName,
                item.unitPrice,
                item.imageUrl,
                item.quantity);
            order._items.Add(orderItem);
        }

        if (order._items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        order.Subtotal = order._items.Sum(i => i.LineTotal);
        order.DiscountAmount = Math.Min(discountAmount, order.Subtotal);
        order.ShippingCost = FlatShippingCost;
        order.Tax = Math.Round((order.Subtotal - order.DiscountAmount) * TaxRate, 2);
        order.Total = order.Subtotal - order.DiscountAmount + order.ShippingCost + order.Tax;

        order.AddDomainEvent(new OrderSubmittedDomainEvent(orderId.Value));

        return order;
    }

    /// <summary>
    /// Marks the order as paid after successful payment processing.
    /// </summary>
    public void MarkAsPaid()
    {
        Status.TransitionTo(OrderStatus.Paid);
        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new OrderPaidDomainEvent(Id.Value));
    }

    /// <summary>
    /// Marks the order as failed with a reason (e.g., stock unavailable, payment declined).
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status.TransitionTo(OrderStatus.Failed);
        Status = OrderStatus.Failed;
        FailureReason = reason;
        AddDomainEvent(new OrderFailedDomainEvent(Id.Value));
    }

    /// <summary>
    /// Confirms the order after all saga steps complete successfully.
    /// </summary>
    public void Confirm()
    {
        Status.TransitionTo(OrderStatus.Confirmed);
        Status = OrderStatus.Confirmed;
    }

    /// <summary>
    /// Marks the order as shipped (admin transition).
    /// Only valid when status is Confirmed.
    /// </summary>
    public void Ship()
    {
        Status.TransitionTo(OrderStatus.Shipped);
        Status = OrderStatus.Shipped;
    }

    /// <summary>
    /// Marks the order as delivered (admin transition).
    /// Only valid when status is Shipped.
    /// </summary>
    public void Deliver()
    {
        Status.TransitionTo(OrderStatus.Delivered);
        Status = OrderStatus.Delivered;
    }

    /// <summary>
    /// Marks that stock has been reserved for this order (saga-internal transition only callable by saga consumers in same assembly).
    /// </summary>
    internal void MarkStockReserved()
    {
        Status.TransitionTo(OrderStatus.StockReserved);
        Status = OrderStatus.StockReserved;
    }
}
