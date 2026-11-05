using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Catalog.Moderation;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Categories;

public sealed record CategoryResponse(Guid Id, string Name, Guid? ParentId)
{
    public static CategoryResponse From(Category c) => new(c.Id.Value, c.Name, c.ParentId?.Value);
}

public sealed record CreateCategoryRequest(string Name, Guid? ParentId);

public sealed record RenameCategoryRequest(Guid Id, string Name);

public sealed record CategoryIdRequest(Guid Id);

public sealed class CreateCategoryValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryValidator() => RuleFor(x => x.Name).Must(CategoryNames.Valid).WithMessage(CategoryNames.Message);
}

public sealed class RenameCategoryValidator : Validator<RenameCategoryRequest>
{
    public RenameCategoryValidator() => RuleFor(x => x.Name).Must(CategoryNames.Valid).WithMessage(CategoryNames.Message);
}

internal static class CategoryNames
{
    public static readonly string Message = $"name must be 1 to {Category.MaxNameLength} characters.";

    public static bool Valid(string? name) => name?.Trim().Length is > 0 and <= Category.MaxNameLength;

    /// <summary>Whether the save hit the unique sibling-name index (FR-034).</summary>
    public static bool IsSiblingNameClash(this DbUpdateException e) => e.UniqueViolationConstraint() == CategoryIndexes.SiblingName;
}

/// <summary>A Platform Operator adds a top-level Category or a child (FR-034, FR-035).</summary>
public sealed class CreateCategoryEndpoint(CatalogDbContext db, ILogger<CreateCategoryEndpoint> logger)
    : Endpoint<CreateCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Post("/categories");
        Group<OpsGroup>();
        Summary(s =>
        {
            s.Summary = "Create a Category";
            s.Responses[201] = "The Category was created.";
            s.Responses[400] = "name is invalid, or parentId is unknown.";
            s.Responses[409] = "category-has-products (the parent holds Products) or category-name-taken (a sibling has that name).";
        });
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        Category? category = null;
        try
        {
            if (req.ParentId is not { } parent)
            {
                category = Category.CreateTopLevel(CategoryId.New(), req.Name);
                db.Categories.Add(category);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                var parentId = CategoryId.From(parent);
                string? refusal = null;
                await CategoryRules.SaveWithCategoryLockedAsync(db, parentId, async (locked, token) =>
                {
                    refusal = null;
                    if (locked is null) refusal = nameof(req.ParentId);
                    else if (await CategoryRules.HoldsProductsAsync(db, parentId, token)) refusal = Problems.CategoryHasProducts;
                    else db.Categories.Add(category ??= Category.CreateChild(CategoryId.New(), req.Name, parentId));
                }, ct);

                if (refusal == nameof(req.ParentId))
                {
                    AddError(r => r.ParentId, "parentId must be an existing Category.");
                    await Send.ErrorsAsync(cancellation: ct);
                    return;
                }

                if (refusal is not null)
                {
                    await Send.ResultAsync(Problems.Conflict(refusal));
                    return;
                }
            }
        }
        catch (DbUpdateException e) when (e.IsSiblingNameClash())
        {
            await Send.ResultAsync(Problems.Conflict(Problems.CategoryNameTaken));
            return;
        }

        logger.LogInformation("Category {CategoryId} created", category!.Id);
        HttpContext.Response.Headers.Location = $"/api/ops/categories/{category.Id.Value}";
        await Send.ResponseAsync(CategoryResponse.From(category), StatusCodes.Status201Created, ct);
    }
}

/// <summary>One Category, for Platform Operators.</summary>
public sealed class GetCategoryEndpoint(CatalogDbContext db) : Endpoint<CategoryIdRequest, CategoryResponse>
{
    public override void Configure()
    {
        Get("/categories/{id}");
        Group<OpsGroup>();
        Summary(s => s.Responses[404] = "No such Category.");
    }

    public override async Task HandleAsync(CategoryIdRequest req, CancellationToken ct)
    {
        var id = CategoryId.From(req.Id);
        var category = await db.Categories.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(CategoryResponse.From(category), ct);
    }
}

/// <summary>A Platform Operator renames a Category; its Products stay in it (FR-034).</summary>
public sealed class RenameCategoryEndpoint(CatalogDbContext db) : Endpoint<RenameCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Put("/categories/{id}");
        Group<OpsGroup>();
        Summary(s =>
        {
            s.Summary = "Rename a Category";
            s.Responses[404] = "No such Category.";
            s.Responses[409] = "category-name-taken: a sibling has that name.";
        });
    }

    public override async Task HandleAsync(RenameCategoryRequest req, CancellationToken ct)
    {
        var id = CategoryId.From(req.Id);
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        category.Rename(req.Name);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.IsSiblingNameClash())
        {
            await Send.ResultAsync(Problems.Conflict(Problems.CategoryNameTaken));
            return;
        }

        await Send.OkAsync(CategoryResponse.From(category), ct);
    }
}

/// <summary>A Platform Operator deletes a Category with neither children nor Products (FR-034).</summary>
public sealed class DeleteCategoryEndpoint(CatalogDbContext db, ILogger<DeleteCategoryEndpoint> logger) : Endpoint<CategoryIdRequest>
{
    public override void Configure()
    {
        Delete("/categories/{id}");
        Group<OpsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a Category";
            s.Responses[204] = "The Category was deleted.";
            s.Responses[404] = "No such Category.";
            s.Responses[409] = "category-has-children or category-has-products.";
        });
    }

    public override async Task HandleAsync(CategoryIdRequest req, CancellationToken ct)
    {
        var id = CategoryId.From(req.Id);
        string? refusal = null;
        var found = false;
        await CategoryRules.SaveWithCategoryLockedAsync(db, id, async (locked, token) =>
        {
            refusal = null;
            found = locked is not null;
            if (locked is null) return;
            if (await CategoryRules.HasChildrenAsync(db, id, token)) refusal = Problems.CategoryHasChildren;
            else if (await CategoryRules.HoldsProductsAsync(db, id, token)) refusal = Problems.CategoryHasProducts;
            else db.Categories.Remove(locked);
        }, ct);

        if (!found)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (refusal is not null)
        {
            await Send.ResultAsync(Problems.Conflict(refusal));
            return;
        }

        logger.LogInformation("Category {CategoryId} deleted", id);
        await Send.NoContentAsync(ct);
    }
}
