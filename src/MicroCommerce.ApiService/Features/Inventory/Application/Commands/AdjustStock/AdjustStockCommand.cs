using MediatR;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid ProductId,
    int Adjustment,
    string? Reason,
    string? AdjustedBy) : IRequest<Unit>;
