using Domain.Interfaces;

namespace Domain.Entities;

public enum PromotionType
{
    FixedDiscount = 0,
    PercentageDiscount = 1
}

public class Promotion : IDateEntity, ISoftDeleteEntity
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public PromotionType Type { get; set; } = PromotionType.FixedDiscount;
    public decimal Discount { get; set; }
    
    public string ShopId { get; set; } = string.Empty;
    public Shop Shop { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}