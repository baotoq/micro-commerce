using MediatR;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;

public sealed record GetStockByProductIdQuery(Guid ProductId) : IRequest<StockInfoDto?>;
