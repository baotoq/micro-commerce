using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;
using MicroCommerce.ApiService.Exceptions;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Product : EntityBase
{
    [MaxLength(500)] public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
    public int TotalStock { get; set; }
    public int SoldQuantity { get; set; }
    public string ImageUrl { get; set; } = "";

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public void UseRemainingStock(int quantity)
    {
        if (RemainingStock < quantity)
        {
            throw CoreException.InvalidArgument("Not enough stock");
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
