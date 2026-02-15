using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;

public sealed class SubmitOrderCommandHandler
    : IRequestHandler<SubmitOrderCommand, Guid>
{
    private readonly OrderingDbContext _context;

    public SubmitOrderCommandHandler(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        SubmitOrderCommand request,
        CancellationToken cancellationToken)
    {
        ShippingAddress address = new(
            request.ShippingAddress.Name,
            request.ShippingAddress.Email,
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.ZipCode);

        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
            request.Items.Select(i => (i.ProductId, i.ProductName, i.UnitPrice, i.ImageUrl, i.Quantity));

        Order order = Order.Create(request.BuyerId, request.Email, address, items);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }
}
