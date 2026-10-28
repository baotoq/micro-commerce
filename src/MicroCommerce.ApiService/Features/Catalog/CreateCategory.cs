using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;

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

/// <summary>Creates a top-level Category, or a child of an existing one that holds no Products.</summary>
public class CreateCategoryEndpoint(CatalogDbContext db) : Endpoint<CreateCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Post("/categories");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var category = new Category { Name = req.Name.Trim(), ParentId = req.ParentId };
        db.Categories.Add(category);

        if (req.ParentId is { } parentId)
        {
            string? conflict = null;
            var parentFound = await db.LockCategoryAsync(parentId, async () =>
            {
                conflict = await db.WhyCannotGainChildAsync(parentId, ct);
                if (conflict is null) await db.SaveChangesAsync(ct);
            }, ct);

            if (!parentFound)
            {
                AddError(r => r.ParentId, CategoryRules.UnknownCategory);
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }
            if (conflict is not null)
            {
                AddError(r => r.ParentId, conflict);
                await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
                return;
            }
        }
        else
        {
            await db.SaveChangesAsync(ct);
        }

        await Send.CreatedAtAsync<GetCategoryEndpoint>(new { id = category.Id }, category.ToResponse(), cancellation: ct);
    }
}
