using System.Collections.Generic;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Category : AuditEntity
    {
        public string? Name { get; set; }

        public IList<ProductCategory> ProductCategories { get; protected set; } = new List<ProductCategory>();
    }
}
