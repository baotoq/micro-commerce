using System.Collections.Generic;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Product : AuditEntity
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public double? RatingAverage { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public IList<ProductCategory> Categories { get; protected set; } = new List<ProductCategory>();

        public IList<Review> Reviews { get; protected set; } = new List<Review>();
    }
}
