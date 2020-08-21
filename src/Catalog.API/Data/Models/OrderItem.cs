using Data.Entities.Models;

namespace Catalog.API.Data.Models
{
    public class OrderItem : Entity
    {
        public long OrderId { get; set; }

        public Order Order { get; set; }

        public long ProductId { get; set; }

        public Product Product { get; set; }

        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }
    }
}
