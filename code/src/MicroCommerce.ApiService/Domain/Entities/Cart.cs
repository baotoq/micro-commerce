﻿using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public enum CartStatus
{
    Pending = 0,
    Paid = 1
}

public class Cart : EntityBase
{
    [MaxLength(Constant.KeyLength)] public Guid? BuyerId { get; set; }
    public Buyer? Buyer { get; set; }

    public CartStatus Status { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TotalPromotionDiscountAmount { get; set; }
    public decimal TotalCheckoutAmount { get; set; }

    [MaxLength(Constant.KeyLength)]public Guid? DeliveryOptionId { get; set; }
    public DeliveryOption? DeliveryOption { get; set; }

    public decimal DeliveryFee { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public Guid? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }

    public Guid? DeliveryAddressId { get; set; }
    public DeliveryAddress? DeliveryAddress { get; set; }

    public DateTimeOffset? CheckoutAt { get; set; }
}
