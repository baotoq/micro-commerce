using Data.Entities.Models;

namespace Ordering.API.Data.Models
{
    public class OrderItem : Entity
    {
        public long OrderId { get; set; }

        public Order Order { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }
    }
}
