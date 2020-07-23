namespace Catalog.API.Application.Orders.Models
{
    public class OrderItemDto
    {
        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }

        public string ProductName { get; set; }
    }
}
