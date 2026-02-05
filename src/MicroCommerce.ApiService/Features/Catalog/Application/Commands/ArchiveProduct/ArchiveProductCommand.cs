using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ArchiveProduct;

public sealed record ArchiveProductCommand(Guid Id) : IRequest<bool>;

