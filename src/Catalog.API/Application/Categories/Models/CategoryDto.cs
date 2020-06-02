using System.Collections.Generic;
using Newtonsoft.Json;

namespace Catalog.API.Application.Categories.Models
{
    public class CategoryDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<ProductDto>? Products { get; set; }
    }
}
