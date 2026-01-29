using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateCategory;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// Catalog module endpoints.
/// Reference implementation for minimal API with CQRS pattern.
/// </summary>
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/catalog")
            .WithTags("Catalog");

        // Category endpoints
        group.MapPost("/categories", CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Create a new category")
            .Produces<CreateCategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/categories", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Get all categories")
            .Produces<IReadOnlyList<CategoryDto>>();

        return endpoints;
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);
        var categoryId = await sender.Send(command, cancellationToken);

        return Results.Created(
            $"/api/catalog/categories/{categoryId.Value}",
            new CreateCategoryResponse(categoryId.Value));
    }

    private static async Task<IResult> GetCategories(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var categories = await sender.Send(query, cancellationToken);

        return Results.Ok(categories);
    }
}

// Request/Response records for endpoint contracts
public sealed record CreateCategoryRequest(string Name, string? Description = null);
public sealed record CreateCategoryResponse(Guid Id);
