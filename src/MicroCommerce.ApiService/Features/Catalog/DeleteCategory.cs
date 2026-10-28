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
        string? conflict = null;
        var found = await db.LockCategoryAsync(req.Id, async () =>
        {
            conflict = await db.WhyCannotBeDeletedAsync(req.Id, ct);
            if (conflict is null) await db.Categories.Where(c => c.Id == req.Id).ExecuteDeleteAsync(ct);
        }, ct);

        if (!found)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        if (conflict is not null)
        {
            AddError(conflict);
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
