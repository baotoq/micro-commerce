using FastEndpoints;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// The Merchant the signed-in Account owns, for Merchant Centre requests. It is resolved from the
/// token, never from the route or body (FR-016, ADR-50).
/// </summary>
public interface ICurrentMerchant
{
    MerchantId Id { get; }

    string ShopName { get; }
}

public sealed class CurrentMerchant : ICurrentMerchant
{
    private MerchantRef? _merchant;

    public MerchantId Id => Merchant.Id;

    public string ShopName => Merchant.ShopName;

    private MerchantRef Merchant =>
        _merchant ?? throw new InvalidOperationException("No Merchant was resolved; is the endpoint in MerchantCentreGroup?");

    public void Set(MerchantRef merchant) => _merchant = merchant;
}

/// <summary>
/// The Merchant Centre, <c>/api/merchant/*</c>: a signed-in Account that owns a Merchant. Anonymous
/// calls get 401 from authorization; an Account without a Merchant gets 403 merchant-required.
/// </summary>
public sealed class MerchantCentreGroup : Group
{
    public MerchantCentreGroup()
    {
        Configure("merchant", ep => ep.PreProcessor<RequireMerchant>(Order.Before));
    }
}

public sealed class RequireMerchant : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        if (http.ResponseStarted()) return;

        var services = http.RequestServices;
        if (!services.GetRequiredService<ICurrentAccount>().TryGet(out var account))
        {
            await Results.Unauthorized().ExecuteAsync(http);
            return;
        }

        var merchant = await services.GetRequiredService<IMerchantDirectory>().FindByAccountAsync(account.Id, ct);
        if (merchant is null)
        {
            await Problems.MerchantRequiredResult().ExecuteAsync(http);
            return;
        }

        services.GetRequiredService<CurrentMerchant>().Set(merchant);
    }
}
