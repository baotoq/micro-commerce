using System.Diagnostics.CodeAnalysis;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>The signed-in Account, as the validated token describes it. Nothing is persisted.</summary>
public sealed record CurrentAccount(AccountId Id, string DisplayName, string? Email, bool IsPlatformOperator);

/// <summary>
/// Request context: handlers get the signed-in Account here, never from the request body (ADR-50).
/// </summary>
public interface ICurrentAccount
{
    bool TryGet([NotNullWhen(true)] out CurrentAccount? account);
}

public sealed class HttpCurrentAccount(IHttpContextAccessor httpContextAccessor) : ICurrentAccount
{
    public bool TryGet([NotNullWhen(true)] out CurrentAccount? account)
    {
        account = null;
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return false;

        // Keycloak's sub is a UUID. A token without one doesn't identify an Account.
        if (!Guid.TryParse(principal.FindFirst("sub")?.Value, out var subject)) return false;

        var displayName = NonEmpty(principal.FindFirst("name")?.Value)
            ?? NonEmpty(principal.FindFirst("preferred_username")?.Value)
            ?? subject.ToString();

        account = new CurrentAccount(
            AccountId.From(subject),
            displayName,
            NonEmpty(principal.FindFirst("email")?.Value),
            principal.FindAll("roles").Any(c => c.Value == AuthPolicies.PlatformOperatorRole));
        return true;
    }

    private static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
