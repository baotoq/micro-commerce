using MediatR;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderDashboard;

public sealed record GetOrderDashboardQuery(
    string TimeRange = "today") : IRequest<OrderDashboardDto>;

public sealed record OrderDashboardDto(
    int TotalOrders,
    decimal Revenue,
    decimal AverageOrderValue,
    int PendingOrders,
    List<DailyOrderCount> OrdersPerDay);

public sealed record DailyOrderCount(
    DateOnly Date,
    int Count);
