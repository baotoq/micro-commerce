namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;

/// <summary>
/// DTO for category data returned by queries.
/// Separates domain model from API contract.
/// </summary>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
