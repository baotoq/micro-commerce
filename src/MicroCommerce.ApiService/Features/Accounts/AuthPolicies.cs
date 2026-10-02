namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// Authorization policies. Keycloak only says who someone is and whether they are a Platform Operator;
/// everything else (such as owning a Merchant) is our own data (ADR-0002).
/// </summary>
public static class AuthPolicies
{
    public const string PlatformOperator = nameof(PlatformOperator);

    /// <summary>The Keycloak realm role, emitted in the access token's <c>roles</c> claim.</summary>
    public const string PlatformOperatorRole = "platform-operator";
}
