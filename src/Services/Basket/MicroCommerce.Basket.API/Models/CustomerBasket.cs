using System.Collections.Generic;

namespace MicroCommerce.Basket.API.Models
{
    public class CustomerBasket
    {
        public string BuyerId { get; set; }

        public List<BasketItem> Items { get; set; } = new();

        public CustomerBasket(string buyerId)
        {
            BuyerId = buyerId;
        }
    }
}
