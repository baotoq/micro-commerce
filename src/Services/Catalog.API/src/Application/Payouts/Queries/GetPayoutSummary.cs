using System.Globalization;
using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Payouts.Queries;

public record GetPayoutSummaryQuery(IClock Clock) : IRequest<PayoutSummaryDto>;

public class GetPayoutSummaryHandler(AppDbContext db) : IRequestHandler<GetPayoutSummaryQuery, PayoutSummaryDto>
{
    public async Task<PayoutSummaryDto> Handle(GetPayoutSummaryQuery request, CancellationToken ct)
    {
        // Net is a computed property (Paid - Fee - LabelCost) that EF cannot translate to SQL,
        // so materialize orders (with their owned Lines) and aggregate in memory.
        // The demo dataset is small (≈46 orders); revisit if order volume grows substantially.
        var orders = await db.Orders.AsNoTracking().ToListAsync(ct);

        var available = orders
            .Where(o => o.Status == OrderStatus.New)
            .Sum(o => o.Net);

        var pending = orders
            .Where(o => o.Status is OrderStatus.Packed or OrderStatus.Shipped)
            .Sum(o => o.Net);

        var nonCancelled = orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .ToList();

        var lifetime = nonCancelled.Sum(o => o.Net);
        var lifetimeOrders = nonCancelled.Count;

        var lifetimeRange = nonCancelled.Count == 0
            ? string.Empty
            : $"{FormatRangeDate(nonCancelled.Min(o => o.PlacedAt))} → {FormatRangeDate(nonCancelled.Max(o => o.PlacedAt))}";

        // AvailableSendsOn = last payout SentAt + 7 days; fall back to clock.Now + 7 when no payout exists.
        var lastPayoutSentAt = await db.Payouts.AsNoTracking()
            .OrderByDescending(p => p.SentAt)
            .Select(p => (DateTimeOffset?)p.SentAt)
            .FirstOrDefaultAsync(ct);

        var availableSendsOn = (lastPayoutSentAt ?? request.Clock.Now).AddDays(7);

        return new PayoutSummaryDto(
            available,
            pending,
            available,
            availableSendsOn,
            lifetime,
            lifetimeOrders,
            lifetimeRange);
    }

    // "Mar 12" style label (TZ-independent, demo offsets are explicit -07:00).
    private static string FormatRangeDate(DateTimeOffset value) =>
        value.ToString("MMM d", CultureInfo.InvariantCulture);
}
