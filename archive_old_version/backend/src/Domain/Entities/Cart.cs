using Domain.Interfaces;

namespace Domain.Entities;

public class Cart : EntityBase, IDateEntity
{
    public string Id { get; set; } = string.Empty;
    
    public string BuyerId { get; set; } = string.Empty;
    public Buyer Buyer { get; set; } = null!;

    public string PromotionId { get; set; } = string.Empty;
    public Promotion? Promotion { get; set; }


    public ICollection<CartProductMap> CartProductMaps { get; set; } = new List<CartProductMap>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void AddProduct(Product product, int quantities)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantities <= 0)
        {
            throw new ArgumentException(null, nameof(quantities));
        }
        
        CartProductMaps.Add(new CartProductMap
        {
            Product = product,
            Quantities = quantities
        });
    }
}