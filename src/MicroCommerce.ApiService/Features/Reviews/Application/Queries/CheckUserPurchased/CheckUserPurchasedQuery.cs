using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Queries.CheckUserPurchased;

public sealed record CheckUserPurchasedQuery(
    Guid UserId,
    Guid ProductId) : IRequest<bool>;

public sealed class CheckUserPurchasedQueryHandler
    : IRequestHandler<CheckUserPurchasedQuery, bool>
{
    private readonly OrderingDbContext _orderingContext;

    public CheckUserPurchasedQueryHandler(OrderingDbContext orderingContext)
    {
        _orderingContext = orderingContext;
    }

    public async Task<bool> Handle(
        CheckUserPurchasedQuery request,
        CancellationToken cancellationToken)
    {
        // Query OrderingDbContext.Orders where BuyerId == UserId,
        // Status in (Paid, Confirmed, Shipped, Delivered),
        // SelectMany Items, AnyAsync where ProductId matches
        return await _orderingContext.Orders
            .Where(o => o.BuyerId == request.UserId)
            .Where(o => o.Status == OrderStatus.Paid ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Shipped ||
                        o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .AnyAsync(item => item.ProductId == request.ProductId, cancellationToken);
    }
}
