using System.Collections.Generic;
using BShop.API.Data.Models.Common;

namespace BShop.API.Data.Models
{
    public class Category : AuditEntity
    {
        public string? Name { get; set; }

        public IList<ProductCategory> ProductCategories { get; protected set; } = new List<ProductCategory>();
    }
}
