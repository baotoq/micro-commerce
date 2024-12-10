using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Product : EntityBase
{
    [MaxLength(500)] public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public long RemainingStock { get; set; }
    public long TotalStock { get; set; }
    public long SoldQuantity { get; set; }
    public string ImageUrl { get; set; } = "";
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
