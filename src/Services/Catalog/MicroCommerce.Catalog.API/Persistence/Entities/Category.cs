using System.Collections.Generic;

namespace MicroCommerce.Catalog.API.Persistence.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Product> Products { get; protected set; }
    }
}
