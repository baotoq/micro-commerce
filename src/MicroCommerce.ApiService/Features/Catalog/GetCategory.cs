using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record GetCategoryRequest(CategoryId Id);

public class GetCategoryEndpoint(CatalogDbContext db) : Endpoint<GetCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Get("/categories/{id:guid}");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
    {
        var category = await db.Categories.Where(c => c.Id == req.Id).ProjectToResponse().SingleOrDefaultAsync(ct);
        if (category is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(category, ct);
    }
}
