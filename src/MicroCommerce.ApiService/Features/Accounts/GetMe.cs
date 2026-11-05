using FastEndpoints;
using Mapster;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>The Merchant an Account owns, as <c>/api/me</c> reports it (FR-006).</summary>
public sealed record MerchantSummary(Guid MerchantId, string ShopName);

/// <summary>The signed-in Account. <see cref="Merchant"/> is null when it owns none (additive in 002).</summary>
public sealed record MeResponse(Guid AccountId, string DisplayName, string? Email, bool IsPlatformOperator, MerchantSummary? Merchant = null);

/// <summary>Returns the signed-in Account, read from the token (FR-008), and the Merchant it owns.</summary>
public sealed class GetMeEndpoint(ICurrentAccount currentAccount, IMerchantDirectory merchants, TypeAdapterConfig mapping)
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

        var merchant = await merchants.FindByAccountAsync(account.Id, ct);
        await Send.OkAsync(
            account.Adapt<MeResponse>(mapping) with
            {
                Merchant = merchant is null ? null : new MerchantSummary(merchant.Id.Value, merchant.ShopName),
            },
            ct);
    }
}
