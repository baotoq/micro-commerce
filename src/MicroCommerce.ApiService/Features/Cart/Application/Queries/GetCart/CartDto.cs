namespace MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCart;

public sealed record CartDto(
    Guid Id,
    List<CartItemDto> Items,
    decimal TotalPrice,
    int TotalItems);

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity,
    decimal LineTotal);
