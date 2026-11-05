using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

public sealed record ListMarketplaceProductsRequest(string? Query, Guid? CategoryId, Guid? ShopId, int? Page, int? PageSize);

public sealed class ListMarketplaceProductsValidator : Validator<ListMarketplaceProductsRequest>
{
    public ListMarketplaceProductsValidator()
    {
        RuleFor(x => x.Query).MaximumLength(200).WithMessage("query must be at most 200 characters.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page must be 1 or more.");
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).WithMessage("pageSize must be 1 or more.");
    }
}

/// <summary>Published Products from every Merchant, searchable and filterable (FR-026 to FR-028).</summary>
public sealed class ListMarketplaceProductsEndpoint(IProductSearch search, IMerchantDirectory merchants)
    : Endpoint<ListMarketplaceProductsRequest, PagedResponse<ProductCard>>
{
    public override void Configure()
    {
        Get("/products");
        Group<MarketplaceGroup>();
        Summary(s =>
        {
            s.Summary = "Browse and search Published Products";
            s.Responses[400] = "page or pageSize below 1, or query over 200 characters.";
        });
    }

    public override async Task HandleAsync(ListMarketplaceProductsRequest req, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(req.Page, req.PageSize);
        var (rows, total) = await search.SearchAsync(new MarketplaceQuery(
            req.Query,
            req.CategoryId is { } c ? CategoryId.From(c) : null,
            req.ShopId is { } s ? MerchantId.From(s) : null,
            page,
            pageSize), ct);

        // One lookup per page for the Shop names (research R1).
        var shops = await merchants.GetShopsAsync(rows.Select(r => r.MerchantId).Distinct().ToList(), ct);
        var cards = rows
            .Where(r => shops.ContainsKey(r.MerchantId))
            .Select(r => new ProductCard(
                r.Id.Value,
                r.Name,
                r.CategoryId.Value,
                r.MinPrice.Value,
                r.MaxPrice.Value,
                r.Available,
                r.CoverImageId is { } cover ? MarketplaceUrls.Image(r.Id, cover) : null,
                new ShopSummary(r.MerchantId.Value, shops[r.MerchantId].ShopName)))
            .ToList();

        await Send.OkAsync(new PagedResponse<ProductCard>(cards, page, pageSize, total), ct);
    }
}
