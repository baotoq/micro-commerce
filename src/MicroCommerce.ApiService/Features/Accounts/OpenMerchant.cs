using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public record OpenMerchantRequest(string ShopName, PickupAddress PickupAddress);

public class OpenMerchantValidator : Validator<OpenMerchantRequest>
{
    public OpenMerchantValidator()
    {
        RuleFor(x => x.ShopName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PickupAddress).NotNull().SetValidator(new PickupAddressValidator());
    }
}

/// <summary>Opens a Merchant for the signed-in Account: immediately, without approval, at most one per Account.</summary>
public class OpenMerchantEndpoint(AccountsDbContext db, CurrentAccount currentAccount)
    : Endpoint<OpenMerchantRequest, MerchantResponse>
{
    public override void Configure()
    {
        Post("/me/merchant");
    }

    public override async Task HandleAsync(OpenMerchantRequest req, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var merchant = new Merchant { AccountId = account.Id, ShopName = req.ShopName.Trim(), PickupAddress = req.PickupAddress };

        if (await db.Merchants.AnyAsync(m => m.AccountId == account.Id, ct))
        {
            await SendConflictAsync(MerchantConfiguration.AccountIdIndex, ct);
            return;
        }
        if (await db.Merchants.AnyAsync(m => m.NormalizedShopName == merchant.NormalizedShopName, ct))
        {
            await SendConflictAsync(MerchantConfiguration.ShopNameIndex, ct);
            return;
        }

        db.Merchants.Add(merchant);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (MerchantUniqueness.ViolatedIndex(e) is { } index)
        {
            // A concurrent request opened a Merchant for this Account, or took the Shop name, first.
            await SendConflictAsync(index, ct);
            return;
        }

        await Send.CreatedAtAsync<GetShopEndpoint>(responseBody: MerchantResponse.From(merchant), cancellation: ct);
    }

    private Task SendConflictAsync(string violatedIndex, CancellationToken ct)
    {
        if (violatedIndex == MerchantConfiguration.ShopNameIndex)
            AddError(r => r.ShopName, MerchantUniqueness.ShopNameTaken);
        else
            AddError(MerchantUniqueness.AlreadyOwnsMerchant);

        return Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
    }
}
