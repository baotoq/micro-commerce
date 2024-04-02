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
    
    public ICollection<CartProductMap> CartProductMaps { get; set; } = new List<CartProductMap>();
}