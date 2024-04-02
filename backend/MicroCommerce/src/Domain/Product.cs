using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Product : DateTimeEntity
{
    [MaxLength(100)]
    public string Id { get; set; } = "";
    [MaxLength(500)]
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
}