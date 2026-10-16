using FastEndpoints;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// The <c>/ops/*</c> route group: what Platform Operators use to run the Marketplace, whichever module it lives
/// in. Only Accounts with the Platform Operator role get in: others get 403, anonymous requests 401.
/// </summary>
public sealed class PlatformOperations : Group
{
    public PlatformOperations()
    {
        Configure("ops", ep => ep.Policies(AuthPolicies.PlatformOperator));
    }
}
