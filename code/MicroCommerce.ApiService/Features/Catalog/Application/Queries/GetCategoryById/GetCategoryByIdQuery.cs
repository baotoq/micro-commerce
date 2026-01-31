using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

