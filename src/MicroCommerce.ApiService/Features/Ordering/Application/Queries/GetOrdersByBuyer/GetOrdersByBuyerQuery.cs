using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrdersByBuyer;

public sealed record GetOrdersByBuyerQuery(
    Guid BuyerId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<OrderListDto>;

public sealed record OrderListDto(
    IReadOnlyList<OrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Total,
    int ItemCount,
    List<string?> ItemThumbnails,
    DateTimeOffset CreatedAt,
    string? FailureReason);
