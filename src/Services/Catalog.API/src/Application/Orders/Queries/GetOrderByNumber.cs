using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders.Queries;

public record GetOrderByNumberQuery(int Number) : IRequest<OrderDetailDto?>;

public class GetOrderByNumberHandler(AppDbContext db) : IRequestHandler<GetOrderByNumberQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(GetOrderByNumberQuery request, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Number == request.Number, ct);
        if (order is null) return null;

        // Lifetime stats: group this customer's non-Cancelled orders by email (R5).
        // Materialize the customer's orders (demo scale) so the computed Paid property is usable.
        var email = order.CustomerEmail;
        var customerOrders = await db.Orders.AsNoTracking()
            .Where(o => o.CustomerEmail == email && o.Status != OrderStatus.Cancelled)
            .ToListAsync(ct);
        var lifetimeOrderCount = customerOrders.Count;
        var lifetimeSpend = customerOrders.Sum(o => o.Paid);

        // Tags left-joined from the Customers table by email (empty when no customer row).
        var customerEmail = Email.From(email);
        var customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == customerEmail, ct);
        IReadOnlyList<string> tags = customer?.Tags.ToList() ?? [];

        var customerDto = new OrderCustomerDto(
            order.CustomerName,
            order.CustomerEmail,
            order.ShipLine1,
            order.ShipLine2,
            order.BillSameAsShip,
            lifetimeOrderCount,
            lifetimeSpend,
            tags);

        return OrderMapping.ToDetailDto(order, customerDto);
    }
}
