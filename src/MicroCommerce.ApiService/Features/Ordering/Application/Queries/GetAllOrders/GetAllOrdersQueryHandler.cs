using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrdersByBuyer;
using MicroCommerce.ApiService.Features.Ordering.Application.Specifications;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
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
        ActiveOrdersSpec spec = new(request.Status);

        IQueryable<Order> query = context.Orders
            .AsNoTracking()
            .WithSpecification(spec);

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
