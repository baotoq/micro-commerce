using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Ordering.Domain.Events;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Entities;

/// <summary>
/// Order aggregate root for the ordering domain.
/// Manages order items, enforces invariants, and tracks order lifecycle.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class Order : BaseAggregateRoot<OrderId>
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
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Order(OrderId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new order from cart items.
    /// Calculates subtotal, shipping, tax, and total automatically.
    /// </summary>
    public static Order Create(
        Guid buyerId,
        string buyerEmail,
        ShippingAddress address,
        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items)
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
            CreatedAt = DateTimeOffset.UtcNow
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
        order.ShippingCost = FlatShippingCost;
        order.Tax = Math.Round(order.Subtotal * TaxRate, 2);
        order.Total = order.Subtotal + order.ShippingCost + order.Tax;

        order.AddDomainEvent(new OrderSubmittedDomainEvent(orderId.Value));

        return order;
    }

    /// <summary>
    /// Marks the order as paid after successful payment processing.
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status is not (OrderStatus.Submitted or OrderStatus.StockReserved))
            throw new InvalidOperationException($"Cannot mark order as paid when status is '{Status}'.");

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

        if (Status is OrderStatus.Confirmed or OrderStatus.Failed)
            throw new InvalidOperationException($"Cannot mark order as failed when status is '{Status}'.");

        Status = OrderStatus.Failed;
        FailureReason = reason;
        AddDomainEvent(new OrderFailedDomainEvent(Id.Value));
    }

    /// <summary>
    /// Confirms the order after all saga steps complete successfully.
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot confirm order when status is '{Status}'.");

        Status = OrderStatus.Confirmed;
    }

    /// <summary>
    /// Marks that stock has been reserved for this order (saga internal transition).
    /// </summary>
    public void MarkStockReserved()
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot mark stock reserved when status is '{Status}'.");

        Status = OrderStatus.StockReserved;
    }
}
