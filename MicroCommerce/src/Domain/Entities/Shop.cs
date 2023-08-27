namespace Domain.Entities;

public class Shop : DateEntity
{
    public string Id { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public Seller Seller { get; set; } = null!;
}