using System.Security.Claims;
using FastEndpoints;
using FluentValidation.Results;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// The <c>/merchant/*</c> route group: endpoints a Merchant uses to manage itself. Every endpoint in it acts
/// for the signed-in Account's own Merchant (read it from <see cref="CurrentMerchant"/>), never for one named
/// in the route or body; Accounts without a Merchant are refused with 403.
/// </summary>
public sealed class MerchantCentre : Group
{
    public MerchantCentre()
    {
        Configure("merchant", ep => ep.PreProcessor<RequireMerchant>(Order.Before));
    }
}

/// <summary>The Merchant a <see cref="MerchantCentre"/> request acts for, resolved from the signed-in Account.</summary>
public class CurrentMerchant(AccountsDbContext db, CurrentAccount currentAccount)
{
    private Merchant? _merchant;

    /// <summary>The signed-in Account's Merchant, tracked by this request's <see cref="AccountsDbContext"/>.</summary>
    public Merchant Merchant => _merchant
        ?? throw new InvalidOperationException($"No Merchant was resolved; is the endpoint in the {nameof(MerchantCentre)} group?");

    public Guid Id => Merchant.Id;

    /// <returns>Whether the signed-in Account owns a Merchant.</returns>
    public async Task<bool> ResolveAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(user, ct);
        _merchant = await db.Merchants.SingleOrDefaultAsync(m => m.AccountId == account.Id, ct);
        return _merchant is not null;
    }
}

public class RequireMerchant : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        if (ctx.HttpContext.ResponseStarted()) return;
        if (await ctx.HttpContext.Resolve<CurrentMerchant>().ResolveAsync(ctx.HttpContext.User, ct)) return;

        await ctx.HttpContext.Response.SendErrorsAsync(
            [new ValidationFailure("", "Only an Account that owns a Merchant can use the Merchant Centre.")],
            StatusCodes.Status403Forbidden,
            cancellation: ct);
    }
}
