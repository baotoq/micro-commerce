using Domain.Interfaces;

namespace Domain.Entities;

public class Seller : IDateEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public string ShopId { get; set; } = "";
    public Shop? Shop { get; set; }
    public ICollection<Product> Products { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}