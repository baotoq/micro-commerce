using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Payouts.Queries;

public record GetPayoutsQuery(int Page, int PageSize) : IRequest<PagedResult<PayoutDto>>;

public class GetPayoutsHandler(AppDbContext db) : IRequestHandler<GetPayoutsQuery, PagedResult<PayoutDto>>
{
    public async Task<PagedResult<PayoutDto>> Handle(GetPayoutsQuery request, CancellationToken ct)
    {
        var query = db.Payouts.AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.SentAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PayoutDto(
                p.Id.Value,
                p.SentAt,
                p.Amount,
                p.Period,
                p.Destination,
                p.IsPending))
            .ToListAsync(ct);

        return new PagedResult<PayoutDto>(items, total, request.Page, request.PageSize);
    }
}
