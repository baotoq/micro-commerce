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
        var (found, conflict) = await db.SaveWithCategoryLockedAsync(req.Id, async () =>
        {
            var reason = await db.WhyCannotBeDeletedAsync(req.Id, ct);
            if (reason is null) db.Categories.Remove(await db.Categories.SingleAsync(c => c.Id == req.Id, ct));
            return reason;
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
