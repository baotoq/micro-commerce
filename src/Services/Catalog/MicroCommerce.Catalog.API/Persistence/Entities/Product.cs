using System.Collections.Generic;

namespace MicroCommerce.Catalog.API.Persistence.Entities
{
    public class Product
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public ICollection<Category> Categories { get; protected set; }
    }
}
