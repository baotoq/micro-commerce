using System.Collections.Generic;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;

namespace Catalog.API.Application.Orders.Models
{
    public class OrderDto
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public decimal SubTotal { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public string OrderNote { get; set; }

        public IList<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
