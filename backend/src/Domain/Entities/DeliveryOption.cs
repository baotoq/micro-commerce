using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class DeliveryOption : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";
    public decimal MinimumSpending { get; set; }
    public decimal Fee { get; set; }

    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}