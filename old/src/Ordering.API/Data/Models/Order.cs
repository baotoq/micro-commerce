using System;
using System.Collections.Generic;
using Dapper;
using Data.Entities.Models;
using Ordering.API.Data.Models.Enums;

namespace Ordering.API.Data.Models
{
    [Table("orders")]
    public class Order : AuditEntity
    {
        public Order()
        {
            OrderStatus = OrderStatus.New;
        }

        [Key]
        [Column("id")]
        public new long Id { get; protected set; }

        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Column("sub_total")]
        public decimal SubTotal { get; set; }

        [Column("order_status")]
        public OrderStatus OrderStatus { get; set; }

        [Column("order_note")]
        public string OrderNote { get; set; }

        [Column("created_date")]
        public new DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("last_modified")]
        public new DateTime LastModified { get; set; } = DateTime.UtcNow;

        public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
