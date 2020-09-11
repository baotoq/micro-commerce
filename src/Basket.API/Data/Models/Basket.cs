using System.Collections.Generic;

namespace Basket.API.Data.Models
{
    public class Basket
    {
        public string CustomerId { get; set; }

        public IList<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
