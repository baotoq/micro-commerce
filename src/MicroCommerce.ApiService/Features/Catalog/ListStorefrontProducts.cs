using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Catalog;

public record ListStorefrontProductsRequest
{
    public const int MaxPageSize = 100;
    public const int QueryMaxLength = 200;

    /// <summary>Words to find in Product names and descriptions, matching their variations; every Product when omitted.</summary>
    public string? Query { get; init; }

    /// <summary>Only Products in this Category or any Category below it; every Category's when omitted.</summary>
    public CategoryId? CategoryId { get; init; }

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
        RuleFor(x => x.Query).MaximumLength(ListStorefrontProductsRequest.QueryMaxLength);
    }
}

/// <summary>
/// The Products Buyers can see, across every Merchant or of one Shop, a page at a time: those matching the query and
/// Category, best matches first when searching and otherwise newest first.
/// </summary>
public class ListStorefrontProductsEndpoint(IProductSearch search)
    : Endpoint<ListStorefrontProductsRequest, PagedResponse<StorefrontProductSummaryResponse>>
{
    public override void Configure()
    {
        Get("/products");
        Group<Storefront>();
    }

    public override async Task HandleAsync(ListStorefrontProductsRequest req, CancellationToken ct) =>
        await Send.OkAsync(
            await search.SearchAsync(
                new()
                {
                    Text = req.Query,
                    CategoryId = req.CategoryId,
                    ShopId = req.ShopId,
                    Page = req.Page,
                    PageSize = req.PageSize,
                },
                ct),
            ct);
}
