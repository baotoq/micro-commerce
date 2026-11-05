namespace MicroCommerce.ApiService.Infrastructure;

/// <summary>
/// The 409 and 403 problems the contract names. Each carries a <c>type</c> the web app branches on
/// (contracts/http-api.md, "Conflict types").
/// </summary>
public static class Problems
{
    public const string MerchantExists = "merchant-exists";
    public const string ShopNameTaken = "shop-name-taken";
    public const string ShopVersionConflict = "shop-version-conflict";
    public const string ProductVersionConflict = "product-version-conflict";
    public const string ProductRemoved = "product-removed";
    public const string CategoryHasProducts = "category-has-products";
    public const string CategoryHasChildren = "category-has-children";
    public const string CategoryNameTaken = "category-name-taken";
    public const string ImageLimitReached = "image-limit-reached";
    public const string MerchantRequired = "merchant-required";

    private static readonly Dictionary<string, string> Titles = new()
    {
        [MerchantExists] = "This Account already owns a Merchant.",
        [ShopNameTaken] = "That Shop name is taken.",
        [ShopVersionConflict] = "The Shop changed since it was read. Reload and try again.",
        [ProductVersionConflict] = "The Product changed since it was read. Reload and try again.",
        [ProductRemoved] = "A Platform Operator removed this Product, so it can't be changed.",
        [CategoryHasProducts] = "The Category holds Products.",
        [CategoryHasChildren] = "The Category has child Categories.",
        [CategoryNameTaken] = "A sibling Category already has that name.",
        [ImageLimitReached] = "A Product can have at most 9 images.",
    };

    /// <summary>A 409 Conflict with one of the contract's conflict types.</summary>
    public static IResult Conflict(string type) =>
        TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: Titles[type], type: type);

    /// <summary>A 403 for a signed-in Account that owns no Merchant.</summary>
    public static IResult MerchantRequiredResult() =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Open a Merchant to use the Merchant Centre.",
            type: MerchantRequired);
}
