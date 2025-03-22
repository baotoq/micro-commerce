using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.ValueObjects;

public class CartItem : ValueObject
{
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public Price PriceAtPurchase { get; }

    public CartItem(Guid productId, int quantity, Price priceAtPurchase)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");

        ProductId = productId;
        Quantity = quantity;
        PriceAtPurchase = priceAtPurchase;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        Quantity += amount;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return Quantity;
        yield return PriceAtPurchase;
    }
}
