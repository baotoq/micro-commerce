using MediatR;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetAdjustmentHistory;

public sealed record GetAdjustmentHistoryQuery(Guid ProductId) : IRequest<List<AdjustmentDto>>;
