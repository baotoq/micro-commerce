using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ChangeProductStatus;

public sealed record ChangeProductStatusCommand(
    Guid Id,
    string Status) : IRequest<bool>;

