namespace MicroCommerce.Catalog.API.Application.Products.Models
{
    public class ProductDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }
    }
}
