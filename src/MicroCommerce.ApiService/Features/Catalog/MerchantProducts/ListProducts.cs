using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

public sealed record ListMerchantProductsRequest(int? Page, int? PageSize);

public sealed class ListMerchantProductsValidator : Validator<ListMerchantProductsRequest>
{
    public ListMerchantProductsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page must be 1 or more.");
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).WithMessage("pageSize must be 1 or more.");
    }
}

/// <summary>Every one of the Merchant's own Products, newest first, with price range and total stock (FR-015).</summary>
public sealed class ListMerchantProductsEndpoint(MerchantCatalog catalog)
    : Endpoint<ListMerchantProductsRequest, PagedResponse<MerchantProductSummary>>
{
    public override void Configure()
    {
        Get("/products");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "The Merchant's own Products, in every status";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
        });
    }

    public override async Task HandleAsync(ListMerchantProductsRequest req, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(req.Page, req.PageSize);
        var query = catalog.Products.AsNoTracking();
        var total = await query.CountAsync(ct);

        // The aggregates are computed by the database (research R12).
        var rows = await query
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.CategoryId,
                p.Status,
                VariantCount = p.Variants.Count(),
                MinPrice = p.Variants.Min(v => v.Price),
                MaxPrice = p.Variants.Max(v => v.Price),
                TotalStock = p.Variants.Sum(v => (long)v.Stock),
                Cover = p.Images.Where(i => i.Position == 0).Select(i => (Domain.Catalog.ProductImageId?)i.Id).FirstOrDefault(),
                p.CreatedAt,
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new MerchantProductSummary(
            r.Id.Value, r.Name, r.CategoryId.Value, r.Status.ToString(), r.VariantCount, r.MinPrice.Value, r.MaxPrice.Value, r.TotalStock,
            r.Cover is { } cover ? MerchantProductResponse.ImageUrl("/api/merchant", r.Id, cover) : null,
            r.CreatedAt)).ToList();

        await Send.OkAsync(new PagedResponse<MerchantProductSummary>(items, page, pageSize, total), ct);
    }
}
