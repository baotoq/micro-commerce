using MediatR;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReserveStock;

public sealed record ReserveStockCommand(
    Guid ProductId,
    int Quantity) : IRequest<Guid>;
