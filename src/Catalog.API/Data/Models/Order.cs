using System.Collections.Generic;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Order : AuditEntity
    {
        public Order()
        {
            OrderStatus = OrderStatus.New;
        }

        public long CustomerId { get; set; }

        public decimal SubTotal { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public string OrderNote { get; set; }

        public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
