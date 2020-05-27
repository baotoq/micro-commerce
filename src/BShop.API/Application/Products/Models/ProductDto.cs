using System.Collections.Generic;
using Newtonsoft.Json;

namespace BShop.API.Application.Products.Models
{
    public class ProductDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? ImageUri { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<ProductCategoryDto>? Categories { get; set; }
    }
}
