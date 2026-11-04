using FastEndpoints;
using Mapster;

namespace MicroCommerce.ApiService.Features.Accounts;

public sealed record MeResponse(Guid AccountId, string DisplayName, string? Email, bool IsPlatformOperator);

/// <summary>Returns the signed-in Account, read from the token (FR-008).</summary>
public sealed class GetMeEndpoint(ICurrentAccount currentAccount, TypeAdapterConfig mapping)
    : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/me");
        Summary(s =>
        {
            s.Summary = "The signed-in Account";
            s.Responses[200] = "The Account described by the bearer token.";
            s.Responses[401] = "No token, an invalid token, or a token whose subject isn't a UUID.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!currentAccount.TryGet(out var account))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(account.Adapt<MeResponse>(mapping), ct);
    }
}
