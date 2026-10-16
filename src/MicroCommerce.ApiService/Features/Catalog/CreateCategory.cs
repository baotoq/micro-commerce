using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <param name="ParentId">The Category to add a child to; null for a top-level Category.</param>
public record CreateCategoryRequest(string Name, CategoryId? ParentId);

public class CreateCategoryValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
    }
}

/// <summary>Creates a top-level Category, or a child of an existing one.</summary>
public class CreateCategoryEndpoint(CatalogDbContext db) : Endpoint<CreateCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Post("/categories");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        if (req.ParentId is { } parentId)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == parentId, ct))
            {
                await SendUnknownParentAsync(ct);
                return;
            }
            if (await db.WhyCannotGainChildAsync(parentId, ct) is { } reason)
            {
                AddError(r => r.ParentId, reason);
                await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
                return;
            }
        }

        var category = new Category { Name = req.Name.Trim(), ParentId = req.ParentId };
        db.Categories.Add(category);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (CategoryRules.IsParentViolation(e))
        {
            // The parent was deleted concurrently.
            await SendUnknownParentAsync(ct);
            return;
        }

        await Send.CreatedAtAsync<GetCategoryEndpoint>(new { id = category.Id }, category.ToResponse(), cancellation: ct);
    }

    private Task SendUnknownParentAsync(CancellationToken ct)
    {
        AddError(r => r.ParentId, CategoryRules.UnknownParent);
        return Send.ErrorsAsync(cancellation: ct);
    }
}
