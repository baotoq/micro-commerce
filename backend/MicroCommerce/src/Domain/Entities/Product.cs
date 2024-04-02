using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class Product : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
    public int TotalStock { get; set; }
    public int SoldQuantity { get; set; }
    
    public ICollection<CartProductMap> CartProductMaps { get; set; } = new List<CartProductMap>();
}