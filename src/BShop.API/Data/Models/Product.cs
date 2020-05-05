using System.Collections.Generic;

namespace BShop.API.Data.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string ImageFileName { get; set; }

        public int BrandId { get; set; }

        public IList<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();
    }
}
