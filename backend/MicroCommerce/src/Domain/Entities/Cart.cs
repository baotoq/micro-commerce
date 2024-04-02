using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class Cart : EntityBase
{
    [MaxLength(Constant.KeyLength)]
    public string BuyerId { get; set; } = "";
    public Buyer Buyer { get; set; } = null!;
    
    public ICollection<CartProductMap> CartProductMaps { get; set; } = new List<CartProductMap>();
}