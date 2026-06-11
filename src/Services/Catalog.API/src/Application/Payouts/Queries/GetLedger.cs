using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Payouts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Payouts.Queries;

public record GetLedgerQuery(int Page, int PageSize) : IRequest<PagedResult<LedgerEntryDto>>;

public class GetLedgerHandler(AppDbContext db) : IRequestHandler<GetLedgerQuery, PagedResult<LedgerEntryDto>>
{
    public async Task<PagedResult<LedgerEntryDto>> Handle(GetLedgerQuery request, CancellationToken ct)
    {
        var query = db.LedgerEntries.AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new LedgerEntryDto(
                e.OccurredAt,
                e.Label,
                e.Subject,
                e.Amount,
                e.Kind == LedgerEntryKind.Sale ? "sale"
                    : e.Kind == LedgerEntryKind.Fee ? "fee"
                    : e.Kind == LedgerEntryKind.Payout ? "payout"
                    : "label"))
            .ToListAsync(ct);

        return new PagedResult<LedgerEntryDto>(items, total, request.Page, request.PageSize);
    }
}
