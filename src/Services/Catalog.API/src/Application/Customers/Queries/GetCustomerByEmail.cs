using MediatR;
using MicroCommerce.Catalog.Application.Customers.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Customers.Queries;

public record GetCustomerByEmailQuery(string Email) : IRequest<CustomerDto?>;

public class GetCustomerByEmailHandler(AppDbContext db) : IRequestHandler<GetCustomerByEmailQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByEmailQuery request, CancellationToken ct)
    {
        var email = Email.From(request.Email);

        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);
        if (customer is null) return null;

        // Lifetime stats are NOT stored — derive them by grouping db.Orders on the
        // CustomerEmail snapshot (no cross-domain FK, see plan D3). Cancelled orders excluded.
        var emailValue = email.Value;
        var stats = await db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerEmail == emailValue && o.Status != OrderStatus.Cancelled)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Spend = g.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Qty) + o.ShippingPaid + o.Tax),
                FirstAt = (DateTimeOffset?)g.Min(o => o.PlacedAt),
                LastAt = (DateTimeOffset?)g.Max(o => o.PlacedAt),
            })
            .FirstOrDefaultAsync(ct);

        return CustomerMapping.ToDto(
            customer,
            stats?.Count ?? 0,
            stats?.Spend ?? 0m,
            stats?.FirstAt,
            stats?.LastAt);
    }
}
