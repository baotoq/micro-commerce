using System.Collections.Generic;

namespace Catalog.API.Application.Carts.Models
{
    public class CartDto
    {
        public IList<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
