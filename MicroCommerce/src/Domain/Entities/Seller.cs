namespace Domain.Entities;

public class Seller : DateEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string ShopId { get; set; } = string.Empty;
    public Shop? Shop { get; set; }
    public ICollection<Product> Products { get; set; } = null!;
}