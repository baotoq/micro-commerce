using System.Collections.Generic;
using Data.Entities.Models;

namespace Catalog.API.Data.Models
{
    public class Cart : AuditEntity
    {
        public Cart()
        {
            IsActive = true;
            LockedOnCheckout = false;
        }

        public string CustomerId { get; set; }

        public bool IsActive { get; set; }

        public string OrderNote { get; set; }

        public bool LockedOnCheckout { get; set; }

        public IList<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
