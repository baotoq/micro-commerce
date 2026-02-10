using MediatR;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;

public sealed record SubmitOrderCommand(
    Guid BuyerId,
    string Email,
    ShippingAddressRequest ShippingAddress,
    List<OrderItemRequest> Items) : IRequest<SubmitOrderResult>;

public sealed record ShippingAddressRequest(
    string Name,
    string Email,
    string Street,
    string City,
    string State,
    string ZipCode);

public sealed record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity);

public sealed record SubmitOrderResult(Guid OrderId, string OrderNumber);
