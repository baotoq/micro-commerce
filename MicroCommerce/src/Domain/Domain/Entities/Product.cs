namespace Domain.Entities;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
}