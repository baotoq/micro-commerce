using System.Collections.Generic;
using Newtonsoft.Json;

namespace Catalog.API.Application.Products.Models
{
    public class ProductDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CartMaxQuantity { get; set; }

        public int ReviewsCount { get; set; }

        public double? RatingAverage { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<CategoryDto> Categories { get; set; }
    }
}
