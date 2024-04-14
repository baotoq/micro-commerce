using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Product : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
    public int TotalStock { get; set; }
    public int SoldQuantity { get; set; }
    
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    
    public void UseRemainingStock(int quantity)
    {
        if (RemainingStock < quantity)
        {
            throw new ArgumentException("Not enough stock");
        }

        RemainingStock -= quantity;
        TotalStock -= quantity;
    }
    
    public void RefundRemainingStock(int quantity)
    {
        RemainingStock += quantity;
        TotalStock += quantity;
    }
}