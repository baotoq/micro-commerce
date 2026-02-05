using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<bool>;

