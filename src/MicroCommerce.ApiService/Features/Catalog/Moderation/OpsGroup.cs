using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.ApiService.Features.Catalog.Moderation;

/// <summary>Platform operations: <c>/api/ops/*</c>, Platform Operators only (401 anonymous, 403 otherwise).</summary>
public sealed class OpsGroup : Group
{
    public OpsGroup()
    {
        Configure("ops", ep => ep.Policies(AuthPolicies.PlatformOperator));
    }
}
