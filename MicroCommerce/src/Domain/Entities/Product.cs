namespace Domain.Entities;

public class Product : AuditEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Seller Seller { get; set; } = null!;
}