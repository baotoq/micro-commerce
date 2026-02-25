using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Entities;

/// <summary>
/// OrderItem entity owned by Order aggregate.
/// Stores a snapshot of product info at the time of ordering.
/// </summary>
public sealed class OrderItem : Entity<OrderItemId>
{
    public OrderId OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public string? ImageUrl { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal { get; private set; }

    // EF Core constructor
    private OrderItem() : base()
    {
    }

    private OrderItem(OrderItemId id) : base(id)
    {
    }

    internal static OrderItem Create(OrderId orderId, Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        return new OrderItem(OrderItemId.New())
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            ImageUrl = imageUrl,
            Quantity = quantity,
            LineTotal = unitPrice * quantity
        };
    }
}
