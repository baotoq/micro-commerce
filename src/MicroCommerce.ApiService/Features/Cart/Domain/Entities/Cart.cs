using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Cart.Domain.Entities;

/// <summary>
/// Cart aggregate root for the cart domain.
/// Manages cart items, enforces quantity invariants, and tracks a 30-day TTL.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class Cart : BaseAggregateRoot<CartId>
{
    private const int MaxQuantity = 99;
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    private readonly List<CartItem> _items = [];

    /// <summary>
    /// BuyerId as raw Guid - supports both authenticated users (sub claim) and guest cookies.
    /// </summary>
    public Guid BuyerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Cart(CartId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new cart for a buyer.
    /// </summary>
    public static Cart Create(Guid buyerId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Cart(CartId.New())
        {
            BuyerId = buyerId,
            CreatedAt = now,
            LastModifiedAt = now,
            ExpiresAt = now.Add(Ttl)
        };
    }

    /// <summary>
    /// Adds a product to the cart. If the product already exists, increments quantity (capped at 99).
    /// </summary>
    public void AddItem(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity = 1)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncrementQuantity(quantity);
        }
        else
        {
            var cappedQuantity = Math.Min(quantity, MaxQuantity);
            var item = CartItem.Create(Id, productId, productName, unitPrice, imageUrl, cappedQuantity);
            _items.Add(item);
        }

        Touch();
    }

    /// <summary>
    /// Updates the quantity of a specific cart item.
    /// </summary>
    public void UpdateItemQuantity(CartItemId itemId, int newQuantity)
    {
        if (newQuantity < 1 || newQuantity > MaxQuantity)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), $"Quantity must be between 1 and {MaxQuantity}.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Cart item '{itemId}' not found.");

        item.SetQuantity(newQuantity);
        Touch();
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public void RemoveItem(CartItemId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is not null)
        {
            _items.Remove(item);
            Touch();
        }
    }

    /// <summary>
    /// Transfers cart ownership to a different buyer (used during guest-to-auth merge).
    /// </summary>
    public void TransferOwnership(Guid newBuyerId)
    {
        BuyerId = newBuyerId;
        Touch();
    }

    /// <summary>
    /// Updates modification timestamp and resets TTL.
    /// </summary>
    private void Touch()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
        ExpiresAt = DateTimeOffset.UtcNow.Add(Ttl);
    }
}
