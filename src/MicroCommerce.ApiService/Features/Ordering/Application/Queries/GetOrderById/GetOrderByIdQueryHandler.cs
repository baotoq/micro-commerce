using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly OrderingDbContext _context;

    public GetOrderByIdQueryHandler(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        OrderId orderId = OrderId.From(request.OrderId);

        Domain.Entities.Order? order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return null;

        List<OrderItemDto> items = order.Items.Select(i => new OrderItemDto(
            i.Id.Value,
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.ImageUrl,
            i.Quantity,
            i.LineTotal)).ToList();

        ShippingAddressDto shippingAddress = new(
            order.ShippingAddress.Name,
            order.ShippingAddress.Email,
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.ZipCode);

        return new OrderDto(
            order.Id.Value,
            order.OrderNumber.Value,
            order.BuyerEmail,
            order.Status,
            shippingAddress,
            items,
            order.Subtotal,
            order.ShippingCost,
            order.Tax,
            order.Total,
            order.CreatedAt,
            order.PaidAt,
            order.FailureReason);
    }
}
