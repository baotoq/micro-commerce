using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Cart.Domain.Entities;

/// <summary>
/// CartItem entity owned by Cart aggregate.
/// Stores a snapshot of product info at the time of adding to cart.
/// </summary>
public sealed class CartItem
{
    private const int MaxQuantity = 99;

    public CartItemId Id { get; private set; } = null!;
    public CartId CartId { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public string? ImageUrl { get; private set; }
    public int Quantity { get; private set; }

    // EF Core constructor
    private CartItem()
    {
    }

    internal static CartItem Create(CartId cartId, Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)
    {
        if (quantity < 1 || quantity > MaxQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity), $"Quantity must be between 1 and {MaxQuantity}.");

        return new CartItem
        {
            Id = CartItemId.New(),
            CartId = cartId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            ImageUrl = imageUrl,
            Quantity = quantity
        };
    }

    public void IncrementQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Increment amount must be positive.");

        Quantity = Math.Min(Quantity + amount, MaxQuantity);
    }

    public void SetQuantity(int newQuantity)
    {
        if (newQuantity < 1 || newQuantity > MaxQuantity)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), $"Quantity must be between 1 and {MaxQuantity}.");

        Quantity = newQuantity;
    }
}
