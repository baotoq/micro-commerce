namespace Domain.Entities;

public class Seller : AuditEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Product> Products { get; set; } = null!;
}