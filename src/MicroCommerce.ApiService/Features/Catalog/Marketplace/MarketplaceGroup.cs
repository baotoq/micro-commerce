using FastEndpoints;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

/// <summary>
/// Public browsing: <c>/api/marketplace/*</c> needs no sign-in (Principle IV: explicitly public).
/// Hidden Products are left out or return 404 instead (FR-025).
/// </summary>
public sealed class MarketplaceGroup : Group
{
    public MarketplaceGroup()
    {
        Configure("marketplace", ep => ep.AllowAnonymous());
    }
}
