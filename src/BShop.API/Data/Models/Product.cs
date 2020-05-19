using System.Collections.Generic;
using BShop.API.Data.Models.Common;

namespace BShop.API.Data.Models
{
    public class Product : AuditEntity
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string ImageFileName { get; set; }

        public IList<ProductCategory> ProductCategories { get; protected set; } = new List<ProductCategory>();

        public IList<Review> Reviews { get; protected set; } = new List<Review>();
    }
}
