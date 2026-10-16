using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record DeleteCategoryRequest(CategoryId Id);

/// <summary>Deletes a Category that has nothing in it: no child Categories and no Products.</summary>
public class DeleteCategoryEndpoint(CatalogDbContext db) : Endpoint<DeleteCategoryRequest>
{
    public override void Configure()
    {
        Delete("/categories/{id:guid}");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == req.Id, ct);
        if (category is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (await db.WhyCannotBeDeletedAsync(category.Id, ct) is { } reason)
        {
            await SendConflictAsync(reason, ct);
            return;
        }

        db.Categories.Remove(category);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (CategoryRules.IsParentViolation(e))
        {
            // A child was added concurrently.
            await SendConflictAsync(CategoryRules.HasChildren, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }

    private Task SendConflictAsync(string reason, CancellationToken ct)
    {
        AddError(reason);
        return Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
    }
}
