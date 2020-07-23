namespace Catalog.API.Application.Orders.Models
{
    public class OrderItemDto
    {
        public long Id { get; set; }

        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }

        public string ProductName { get; set; }
    }
}
