using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    string BuyerEmail,
    OrderStatus Status,
    ShippingAddressDto ShippingAddress,
    List<OrderItemDto> Items,
    decimal Subtotal,
    decimal ShippingCost,
    decimal Tax,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    string? FailureReason);

public sealed record ShippingAddressDto(
    string Name,
    string Email,
    string Street,
    string City,
    string State,
    string ZipCode);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity,
    decimal LineTotal);
