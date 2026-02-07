using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrdersByBuyer;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 50) : IRequest<OrderListDto>;
