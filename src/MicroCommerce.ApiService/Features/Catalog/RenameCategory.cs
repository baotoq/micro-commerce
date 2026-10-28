using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record RenameCategoryRequest(CategoryId Id, string Name);

public class RenameCategoryValidator : Validator<RenameCategoryRequest>
{
    public RenameCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
    }
}

/// <summary>Renames a Category; it keeps its place in the tree, its children and its Products.</summary>
public class RenameCategoryEndpoint(CatalogDbContext db) : Endpoint<RenameCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Put("/categories/{id:guid}");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(RenameCategoryRequest req, CancellationToken ct)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == req.Id, ct);
        if (category is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        category.Name = req.Name.Trim();
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(category.ToResponse(), ct);
    }
}
