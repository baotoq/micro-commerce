using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders.Queries;

public record GetOrdersQuery(int Page, int PageSize, string? Tab) : IRequest<PagedResult<OrderInboxRowDto>>;

public class GetOrdersHandler(AppDbContext db) : IRequestHandler<GetOrdersQuery, PagedResult<OrderInboxRowDto>>
{
    public async Task<PagedResult<OrderInboxRowDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders.AsNoTracking();

        query = request.Tab?.ToLowerInvariant() switch
        {
            "needs-action" => query.Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.RefundRequested),
            "packed" => query.Where(o => o.Status == OrderStatus.Packed),
            "shipped" => query.Where(o => o.Status == OrderStatus.Shipped),
            "delivered" => query.Where(o => o.Status == OrderStatus.Delivered),
            "refund-cancel" => query.Where(o => o.Status == OrderStatus.RefundRequested || o.Status == OrderStatus.Cancelled),
            _ => query, // "all" or null/unknown -> no filter
        };

        var total = await query.CountAsync(ct);
        var orders = await query
            .OrderByDescending(o => o.PlacedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(OrderMapping.ToInboxRowDto).ToList();
        return new PagedResult<OrderInboxRowDto>(items, total, request.Page, request.PageSize);
    }
}
