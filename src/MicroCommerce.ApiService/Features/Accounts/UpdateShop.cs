using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public record UpdateShopRequest(string ShopName, string? Description, PickupAddress PickupAddress);

public class UpdateShopValidator : Validator<UpdateShopRequest>
{
    public UpdateShopValidator()
    {
        RuleFor(x => x.ShopName).NotEmpty().MaximumLength(ShopName.MaxLength);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PickupAddress).NotNull().SetValidator(new PickupAddressValidator());
    }
}

/// <summary>Replaces the Merchant's Shop profile: Shop name, description and Pickup Address.</summary>
public class UpdateShopEndpoint(AccountsDbContext db, CurrentMerchant currentMerchant)
    : Endpoint<UpdateShopRequest, MerchantResponse>
{
    public override void Configure()
    {
        Put("/shop");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(UpdateShopRequest req, CancellationToken ct)
    {
        var merchant = currentMerchant.Merchant;
        merchant.ShopName = ShopName.From(req.ShopName);
        merchant.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        merchant.PickupAddress = req.PickupAddress;

        var shopNameTaken = await db.Merchants.AnyAsync(
            m => m.Id != merchant.Id && m.NormalizedShopName == merchant.NormalizedShopName, ct);
        if (!shopNameTaken)
        {
            try
            {
                await db.SaveChangesAsync(ct);
                await Send.OkAsync(merchant.ToResponse(), ct);
                return;
            }
            catch (DbUpdateException e) when (MerchantUniqueness.ViolatedIndex(e) == MerchantConfiguration.ShopNameIndex)
            {
                // A concurrent request took the Shop name first.
            }
        }

        AddError(r => r.ShopName, MerchantUniqueness.ShopNameTaken);
        await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
    }
}
