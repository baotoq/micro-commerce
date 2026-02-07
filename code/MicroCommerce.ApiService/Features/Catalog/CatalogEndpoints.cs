using MediatR;
using Microsoft.AspNetCore.Mvc;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.ArchiveProduct;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.ChangeProductStatus;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateCategory;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateProduct;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.DeleteCategory;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateCategory;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateProduct;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.UploadProductImage;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategoryById;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProductById;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

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

        group.MapGet("/categories/{id:guid}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Get a category by ID")
            .Produces<CategoryDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/categories/{id:guid}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Update a category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/categories/{id:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Delete a category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Product endpoints
        group.MapGet("/products", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get products with filtering and pagination")
            .Produces<ProductListDto>();

        group.MapGet("/products/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID")
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/products", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/products/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/products/{id:guid}/status", ChangeProductStatus)
            .WithName("ChangeProductStatus")
            .WithSummary("Change product status (publish/unpublish)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/products/{id:guid}", ArchiveProduct)
            .WithName("ArchiveProduct")
            .WithSummary("Archive a product (soft delete)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Image upload endpoint
        group.MapPost("/images", UploadImage)
            .WithName("UploadProductImage")
            .WithSummary("Upload a product image")
            .Produces<UploadImageResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .DisableAntiforgery();

        return endpoints;
    }

    // Category handlers
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

    private static async Task<IResult> GetCategoryById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> UpdateCategory(
        Guid id,
        UpdateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Description);
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return Results.NoContent();
    }

    // Product handlers
    private static async Task<IResult> GetProducts(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery(
            page == 0 ? 1 : page,
            pageSize == 0 ? 20 : pageSize,
            categoryId,
            status,
            search,
            sortBy,
            sortDirection);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.ImageUrl,
            request.Sku);

        var productId = await sender.Send(command, cancellationToken);

        return Results.Created(
            $"/api/catalog/products/{productId}",
            new CreateProductResponse(productId));
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.ImageUrl,
            request.Sku);

        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeProductStatus(
        Guid id,
        ChangeProductStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ChangeProductStatusCommand(id, request.Status);
        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ArchiveProduct(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ArchiveProductCommand(id), cancellationToken);
        return Results.NoContent();
    }

    // Image upload handler
    private static async Task<IResult> UploadImage(
        IFormFile file,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var command = new UploadProductImageCommand(
            stream,
            file.FileName,
            file.ContentType,
            file.Length);

        var imageUrl = await sender.Send(command, cancellationToken);

        return Results.Created(imageUrl, new UploadImageResponse(imageUrl));
    }
}

// Request/Response records for endpoint contracts
public sealed record CreateCategoryRequest(string Name, string? Description = null);

public sealed record CreateCategoryResponse(Guid Id);

public sealed record UpdateCategoryRequest(string Name, string? Description = null);

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl = null,
    string? Sku = null);

public sealed record CreateProductResponse(Guid Id);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl = null,
    string? Sku = null);

public sealed record ChangeProductStatusRequest(string Status);

public sealed record UploadImageResponse(string ImageUrl);
