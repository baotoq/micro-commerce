using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Integration.Builders;

/// <summary>
/// Fluent builder for creating Order entities in integration tests.
/// Produces orders in Submitted status (the initial state after checkout).
/// Provides sensible defaults that can be overridden for specific test scenarios.
/// </summary>
public sealed class OrderBuilder
{
    private Guid _buyerId = Guid.NewGuid();
    private string _buyerEmail = "test@example.com";
    private ShippingAddress _shippingAddress = new(
        "Test User",
        "test@example.com",
        "123 Test St",
        "Seattle",
        "WA",
        "98101");

    private List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> _items =
    [
        (Guid.NewGuid(), "Test Product", 49.99m, null, 1)
    ];

    public OrderBuilder WithBuyerId(Guid buyerId)
    {
        _buyerId = buyerId;
        return this;
    }

    public OrderBuilder WithBuyerEmail(string buyerEmail)
    {
        _buyerEmail = buyerEmail;
        return this;
    }

    public OrderBuilder WithShippingAddress(ShippingAddress shippingAddress)
    {
        _shippingAddress = shippingAddress;
        return this;
    }

    public OrderBuilder WithItem(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)
    {
        _items = [(productId, productName, unitPrice, imageUrl, quantity)];
        return this;
    }

    public OrderBuilder AddItem(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)
    {
        _items.Add((productId, productName, unitPrice, imageUrl, quantity));
        return this;
    }

    public Order Build()
    {
        return Order.Create(_buyerId, _buyerEmail, _shippingAddress, _items);
    }
}
