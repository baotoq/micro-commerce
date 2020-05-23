using System.Collections.Generic;

namespace BShop.API.Application.Products.Models
{
    public class ProductDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public IList<ProductCategoryDto>? Categories { get; set; }
    }
}
