namespace Catalog.API.Application.Carts.Models
{
    public class CartItemDto
    {
        public long Id { get; set; }

        public ProductDto Product { get; set; }

        public int Quantity { get; set; }
    }
}
