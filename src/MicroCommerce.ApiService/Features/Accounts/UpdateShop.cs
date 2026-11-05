using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public sealed record UpdateShopRequest(string ShopName, string? Description, PickupAddressDto PickupAddress, int? Version);

public sealed class UpdateShopValidator : Validator<UpdateShopRequest>
{
    public UpdateShopValidator()
    {
        ShopProfileRules.AddRules(this, x => x.ShopName, x => x.Description, x => x.PickupAddress);
        RuleFor(x => x.Version).NotNull().GreaterThan(0).WithMessage("version is required.");
    }
}

/// <summary>The owner edits its Shop name, description and Pickup Address (FR-005).</summary>
public sealed class UpdateShopEndpoint(ICurrentMerchant currentMerchant, AccountsDbContext db)
    : Endpoint<UpdateShopRequest, ShopProfileResponse>
{
    public override void Configure()
    {
        Put("/shop");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Edit the signed-in Merchant's Shop profile";
            s.Responses[400] = "A field is missing or invalid.";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
            s.Responses[409] = "shop-name-taken, or shop-version-conflict when version is stale.";
        });
    }

    public override async Task HandleAsync(UpdateShopRequest req, CancellationToken ct)
    {
        var merchant = await db.Merchants.SingleAsync(m => m.Id == currentMerchant.Id, ct);
        if (merchant.Version != req.Version)
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ShopVersionConflict));
            return;
        }

        var shopName = ShopName.From(req.ShopName);
        if (await db.Merchants.AnyAsync(m => m.NormalizedShopName == shopName.Normalized && m.Id != merchant.Id, ct))
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ShopNameTaken));
            return;
        }

        merchant.ChangeProfile(shopName, req.Description, req.PickupAddress.ToDomain());

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ShopVersionConflict));
            return;
        }
        catch (DbUpdateException e) when (e.UniqueViolationConstraint() == MerchantIndexes.NormalizedShopName)
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ShopNameTaken));
            return;
        }

        await Send.OkAsync(ShopProfileResponse.From(merchant), ct);
    }
}
