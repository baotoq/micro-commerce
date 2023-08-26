namespace Domain.Entities;

public class Cart : DateEntity
{
    public string Id { get; set; } = string.Empty;
    
    public string BuyerId { get; set; } = string.Empty;
    public Buyer Buyer { get; set; } = null!;
}