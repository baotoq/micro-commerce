namespace Domain.Entities;

public class Product : EntityBase
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public Seller Seller { get; set; } = null!;
}