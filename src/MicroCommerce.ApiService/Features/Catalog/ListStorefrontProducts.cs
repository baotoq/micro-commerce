using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record ListStorefrontProductsRequest
{
    public const int MaxPageSize = 100;

    /// <summary>1-based.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 24;

    /// <summary>Only this Shop's Products, for its Shop page; every Shop's when omitted.</summary>
    public MerchantId? ShopId { get; init; }
}

public class ListStorefrontProductsValidator : Validator<ListStorefrontProductsRequest>
{
    public ListStorefrontProductsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, ListStorefrontProductsRequest.MaxPageSize);
    }
}

/// <summary>The Products Buyers can see, across every Merchant or of one Shop, newest first, a page at a time.</summary>
public class ListStorefrontProductsEndpoint(CatalogDbContext db)
    : Endpoint<ListStorefrontProductsRequest, PagedResponse<StorefrontProductSummaryResponse>>
{
    public override void Configure()
    {
        Get("/products");
        Group<Storefront>();
    }

    public override async Task HandleAsync(ListStorefrontProductsRequest req, CancellationToken ct)
    {
        var products = db.VisibleProducts();
        if (req.ShopId is { } shopId) products = products.Where(x => x.Shop.Id == shopId);

        var totalCount = await products.CountAsync(ct);
        var items = await products
            .OrderByDescending(x => x.Product.CreatedAt)
            .ThenByDescending(x => x.Product.Id)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ProjectToStorefrontSummaryResponse()
            .ToListAsync(ct);

        await Send.OkAsync(new(items, req.Page, req.PageSize, totalCount), ct);
    }
}
