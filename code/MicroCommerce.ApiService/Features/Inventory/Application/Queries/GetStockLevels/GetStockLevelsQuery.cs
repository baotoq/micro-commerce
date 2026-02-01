using MediatR;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockLevels;

public sealed record GetStockLevelsQuery(List<Guid> ProductIds) : IRequest<List<StockInfoDto>>;
