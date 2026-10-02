using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace MicroCommerce.ApiService.Features.Accounts;

public record MeResponse(Guid Id, string? Email, string DisplayName, bool IsPlatformOperator);

public class GetMeEndpoint(CurrentAccount currentAccount, IAuthorizationService authorization)
    : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var isOperator = (await authorization.AuthorizeAsync(User, AuthPolicies.PlatformOperator)).Succeeded;

        await Send.OkAsync(new MeResponse(account.Id, account.Email, account.DisplayName, isOperator), ct);
    }
}
