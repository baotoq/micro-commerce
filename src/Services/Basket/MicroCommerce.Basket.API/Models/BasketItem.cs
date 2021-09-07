namespace MicroCommerce.Basket.API.Models;

public class BasketItem
{
    public string Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; }
}
