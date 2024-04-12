using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public enum CartStatus
{
    Pending = 0,
    Paid = 1
}

public class Cart : EntityBase
{
    [MaxLength(Constant.KeyLength)]
    public string BuyerId { get; set; } = "";
    public Buyer Buyer { get; set; } = null!;
    
    public CartStatus Status { get; set; }
    
    public decimal SubTotal { get; set; }
    public decimal TotalPromotionDiscountAmount { get; set; }
    public decimal TotalCheckoutAmount { get; set; }
    
    [MaxLength(Constant.KeyLength)]
    public string DeliveryOptionId { get; set; } = "";
    public DeliveryOption DeliveryOption { get; set; } = null!;

    public decimal DeliveryFee { get; set; }
    
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public string PromotionId { get; set; } = "";
    public Promotion Promotion { get; set; } = null!;
    
    public string DeliveryAddressId { get; set; } = "";
    public DeliveryAddress DeliveryAddress { get; set; } = null!;
}