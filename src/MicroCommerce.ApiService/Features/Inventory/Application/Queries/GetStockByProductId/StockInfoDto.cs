namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;

public sealed record StockInfoDto(
    Guid ProductId,
    int QuantityOnHand,
    int AvailableQuantity,
    bool IsInStock,
    bool IsLowStock);
