using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateCategory;

/// <summary>
/// Command to create a new category.
/// Returns the created CategoryId.
/// </summary>
public sealed record CreateCategoryCommand(string Name, string? Description = null) : IRequest<CategoryId>;
