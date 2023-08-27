using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class Product : EntityBase, IDateEntity, ISoftDeleteEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public Seller Seller { get; set; } = null!;

    public ICollection<CartProductMap> CartProductMaps { get; set; } = new List<CartProductMap>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}