using System.Collections.Generic;
using Catalog.API.Data.Models.Enums;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Order : AuditEntity
    {
        public Order()
        {
            OrderStatus = OrderStatus.New;
        }

        public string CustomerId { get; set; }

        public decimal SubTotal { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public string OrderNote { get; set; }

        public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
