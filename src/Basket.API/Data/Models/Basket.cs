using System.Collections.Generic;
using Data.Entities.Models;

namespace Basket.API.Data.Models
{
    public class Basket : Entity
    {
        public long CustomerId { get; set; }

        public IList<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
