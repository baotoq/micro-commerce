using Data.Entities.Models;

namespace Catalog.API.Data.Models
{
    public class CartItem : Entity
    {
        public long ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public long CartId { get; set; }

        public Cart Cart { get; set; }
    }
}
