namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

public enum OrderStatus
{
    Submitted,
    StockReserved,
    Paid,
    Confirmed,
    Shipped,
    Delivered,
    Failed,
    Cancelled
}
