using Domain.Interfaces;

namespace Domain.Entities;

public class Cart : IDateEntity
{
    public string Id { get; set; } = string.Empty;
    
    public string BuyerId { get; set; } = string.Empty;
    public Buyer Buyer { get; set; } = null!;

    public string PromotionId { get; set; } = string.Empty;
    public Promotion? Promotion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}