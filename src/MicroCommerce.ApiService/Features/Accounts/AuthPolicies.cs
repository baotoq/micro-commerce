namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>Authorization policies. Keycloak says who an Account is and whether it is a Platform Operator.</summary>
public static class AuthPolicies
{
    public const string PlatformOperator = nameof(PlatformOperator);

    /// <summary>The Keycloak realm role, emitted in the access token's <c>roles</c> claim.</summary>
    public const string PlatformOperatorRole = "platform-operator";
}
