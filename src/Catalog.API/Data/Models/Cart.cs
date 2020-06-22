using System.Collections.Generic;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Cart : AuditEntity
    {
        public Cart()
        {
            IsActive = true;
        }

        public long CustomerId { get; set; }

        public bool IsActive { get; set; }

        public IList<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
