using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;

/// <summary>
/// Query to retrieve all categories.
/// Returns a list of CategoryDto.
/// </summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
