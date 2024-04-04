namespace Domain.Entities;

public class CartItem
{
    public string CartId { get; set; } = "";
    public Cart Cart { get; set; } = null!;

    public string ProductId { get; set; } = "";
    public Product Product { get; set; } = null!;
    
    public int ProductQuantity { get; set; }
}