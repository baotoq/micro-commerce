using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Accounts;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public sealed record OpenMerchantRequest(string ShopName, string? Description, PickupAddressDto PickupAddress);

public sealed class OpenMerchantValidator : Validator<OpenMerchantRequest>
{
    public OpenMerchantValidator() =>
        ShopProfileRules.AddRules(this, x => x.ShopName, x => x.Description, x => x.PickupAddress);
}

/// <summary>An Account without a Merchant opens one, and can sell at once (FR-001 to FR-003).</summary>
public sealed class OpenMerchantEndpoint(
    ICurrentAccount currentAccount,
    AccountsDbContext db,
    TimeProvider timeProvider,
    ILogger<OpenMerchantEndpoint> logger) : Endpoint<OpenMerchantRequest, ShopProfileResponse>
{
    public override void Configure()
    {
        Post("/me/merchant");
        Summary(s =>
        {
            s.Summary = "Open a Merchant for the signed-in Account";
            s.Responses[201] = "The Merchant was opened.";
            s.Responses[400] = "A field is missing or invalid.";
            s.Responses[401] = "Not signed in.";
            s.Responses[409] = "merchant-exists: the Account already owns one. shop-name-taken: the Shop name is taken.";
        });
    }

    public override async Task HandleAsync(OpenMerchantRequest req, CancellationToken ct)
    {
        if (!currentAccount.TryGet(out var account))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var shopName = ShopName.From(req.ShopName);
        // Friendly checks first; the unique indexes settle races (research R3).
        if (await db.Merchants.AnyAsync(m => m.AccountId == account.Id, ct))
        {
            await Send.ResultAsync(Problems.Conflict(Problems.MerchantExists));
            return;
        }

        if (await db.Merchants.AnyAsync(m => m.NormalizedShopName == shopName.Normalized, ct))
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ShopNameTaken));
            return;
        }

        var merchant = Merchant.Open(
            MerchantId.New(), account.Id, shopName, req.Description, req.PickupAddress.ToDomain(), timeProvider.GetUtcNow());
        db.Merchants.Add(merchant);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.UniqueViolationConstraint() is { } constraint)
        {
            await Send.ResultAsync(Problems.Conflict(
                constraint == MerchantIndexes.AccountId ? Problems.MerchantExists : Problems.ShopNameTaken));
            return;
        }

        logger.LogInformation("Merchant {MerchantId} opened for Account {AccountId}", merchant.Id, account.Id);
        HttpContext.Response.Headers.Location = $"/api/marketplace/shops/{merchant.Id.Value}";
        await Send.ResponseAsync(ShopProfileResponse.From(merchant), StatusCodes.Status201Created, ct);
    }
}
