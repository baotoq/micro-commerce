using MediatR;
using MicroCommerce.Catalog.Application.Customers.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Customers.Queries;

public record GetCustomersQuery(int Page, int PageSize, string? Search) : IRequest<PagedResult<CustomerDto>>;

public class GetCustomersHandler(AppDbContext db) : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var query = db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            // ILike for the Postgres path; case-insensitive Contains fallback for the
            // in-memory provider (which cannot translate EF.Functions.ILike).
            query = db.Database.IsNpgsql()
                ? query.Where(c =>
                    EF.Functions.ILike(c.Name, $"%{search}%") ||
                    EF.Functions.ILike(c.Email.Value, $"%{search}%"))
                : query.Where(c =>
                    c.Name.ToLower().Contains(search.ToLower()) ||
                    c.Email.Value.ToLower().Contains(search.ToLower()));
        }

        var customers = await query.ToListAsync(ct);

        // LEFT-join aggregate stats: group db.Orders on the CustomerEmail snapshot
        // (no cross-domain FK, see plan D3). Cancelled orders excluded from lifetime totals.
        var stats = await db.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.CustomerEmail)
            .Select(g => new
            {
                Email = g.Key,
                Count = g.Count(),
                Spend = g.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Qty) + o.ShippingPaid + o.Tax),
                FirstAt = (DateTimeOffset?)g.Min(o => o.PlacedAt),
                LastAt = (DateTimeOffset?)g.Max(o => o.PlacedAt),
            })
            .ToListAsync(ct);

        var statsByEmail = stats.ToDictionary(s => s.Email);

        var rows = customers
            .Select(c =>
            {
                statsByEmail.TryGetValue(c.Email.Value, out var s);
                return CustomerMapping.ToDto(
                    c,
                    s?.Count ?? 0,
                    s?.Spend ?? 0m,
                    s?.FirstAt,
                    s?.LastAt);
            })
            .OrderByDescending(d => d.LifetimeSpend)
            .ThenBy(d => d.Name)
            .ToList();

        var total = rows.Count;
        var items = rows
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<CustomerDto>(items, total, request.Page, request.PageSize);
    }
}
