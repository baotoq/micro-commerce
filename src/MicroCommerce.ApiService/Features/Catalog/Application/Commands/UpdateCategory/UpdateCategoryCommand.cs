using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null) : IRequest<bool>;

