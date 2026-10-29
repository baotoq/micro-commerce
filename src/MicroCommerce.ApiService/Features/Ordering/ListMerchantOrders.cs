using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Ordering.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering;

public record ListMerchantOrdersRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class ListMerchantOrdersValidator : Validator<ListMerchantOrdersRequest>
{
    public ListMerchantOrdersValidator()
    {
        RuleFor(request => request.Page).GreaterThan(0);
        RuleFor(request => request.Page).LessThanOrEqualTo(int.MaxValue / 100);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}

public class ListMerchantOrdersEndpoint(OrderingDbContext db, CurrentMerchant currentMerchant)
    : Endpoint<ListMerchantOrdersRequest, OrderListResponse>
{
    public override void Configure()
    {
        Get("/orders");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(ListMerchantOrdersRequest request, CancellationToken ct)
    {
        var query = db.Orders.AsNoTracking()
            .Where(order => order.MerchantId == currentMerchant.Id && order.Status == OrderStatus.ToShip);
        var totalCount = await query.CountAsync(ct);
        var page = await query
            .OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(order => order.CheckoutId)
            .ToArrayAsync(ct);
        var items = await OrderResponses.LoadAsync(db, page, currentMerchant.Id, ct);
        await Send.OkAsync(new OrderListResponse(items, request.Page, request.PageSize, totalCount), ct);
    }
}
