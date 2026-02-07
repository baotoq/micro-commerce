using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrdersByBuyer;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryHandler(OrderingDbContext context)
    : IRequestHandler<GetAllOrdersQuery, OrderListDto>
{
    public async Task<OrderListDto> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Order> query = context.Orders
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out OrderStatus statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<OrderSummaryDto> items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id.Value,
                o.OrderNumber.Value,
                o.Status,
                o.Total,
                o.Items.Count(),
                o.Items.Take(3).Select(i => i.ImageUrl).ToList(),
                o.CreatedAt,
                o.FailureReason))
            .ToListAsync(cancellationToken);

        return new OrderListDto(items, totalCount, request.Page, request.PageSize);
    }
}
