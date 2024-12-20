using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Promotion : EntityBase
{
    [MaxLength(500)] public string Name { get; set; } = "";

    public decimal FixedDiscount { get; set; }

    public decimal MinimumSpending { get; set; }
    public decimal MaximumDiscount { get; set; }

    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
