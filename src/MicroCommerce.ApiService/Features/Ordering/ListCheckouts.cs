using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Ordering.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering;

public record ListCheckoutsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class ListCheckoutsValidator : Validator<ListCheckoutsRequest>
{
    public ListCheckoutsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Page).LessThanOrEqualTo(int.MaxValue / 100);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class ListCheckoutsEndpoint(OrderingDbContext db, CurrentAccount currentAccount)
    : Endpoint<ListCheckoutsRequest, CheckoutListResponse>
{
    public override void Configure() => Get("/checkouts");

    public override async Task HandleAsync(ListCheckoutsRequest request, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var checkouts = db.Checkouts.AsNoTracking().Where(c => c.AccountId == account.Id);
        var totalCount = await checkouts.CountAsync(ct);
        var items = await checkouts
            .OrderByDescending(c => c.PlacedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(c => c.Merchants)
            .Include(c => c.Lines)
            .ToListAsync(ct);
        var orders = await OrderResponses.LoadAsync(db, items.Select(checkout => checkout.Id).ToArray(), null, ct);
        var ordersByCheckout = orders.GroupBy(order => order.CheckoutId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<OrderResponse>)group.ToArray());
        await Send.OkAsync(new CheckoutListResponse(
            items.Select(checkout => checkout.ToCheckoutResponse() with
            {
                Orders = ordersByCheckout.GetValueOrDefault(checkout.Id, []),
            }).ToList(),
            request.Page,
            request.PageSize,
            totalCount), ct);
    }
}
