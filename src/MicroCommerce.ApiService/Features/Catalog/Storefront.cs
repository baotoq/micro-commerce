using FastEndpoints;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>The <c>/storefront/*</c> route group: what anyone can browse on the Marketplace, signed in or not.</summary>
public sealed class Storefront : Group
{
    public Storefront()
    {
        Configure("storefront", ep => ep.AllowAnonymous());
    }
}
