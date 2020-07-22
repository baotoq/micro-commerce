namespace Catalog.API.Application.Categories.Models
{
    public class ProductDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CartMaxQuantity { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }
    }
}
