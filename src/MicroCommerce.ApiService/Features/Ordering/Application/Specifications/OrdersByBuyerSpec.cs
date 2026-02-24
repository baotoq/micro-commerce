using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Specifications;

/// <summary>
/// Specification that filters orders by buyer ID.
/// Single-responsibility: buyer filtering only.
/// Compose with ActiveOrdersSpec for active-order buyer queries.
/// </summary>
public sealed class OrdersByBuyerSpec : Specification<Order>
{
    public OrdersByBuyerSpec(Guid buyerId)
    {
        Query.Where(o => o.BuyerId == buyerId);
    }
}
