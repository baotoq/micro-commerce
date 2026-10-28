using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

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

/// <summary>
/// Replaces the Merchant's Shop profile: Shop name, description and Pickup Address. Publishes
/// <see cref="MerchantProfileChanged"/> in the same transaction.
/// </summary>
public class UpdateShopEndpoint(IDbContextOutbox<AccountsDbContext> outbox, CurrentMerchant currentMerchant)
    : Endpoint<UpdateShopRequest, MerchantResponse>
{
    public override void Configure()
    {
        Put("/shop");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(UpdateShopRequest req, CancellationToken ct)
    {
        // CurrentMerchant tracks the Merchant in this request's context, which is the outbox's.
        var merchant = currentMerchant.Merchant;
        merchant.ChangeProfile(
            ShopName.From(req.ShopName),
            string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            req.PickupAddress);

        var shopNameTaken = await outbox.DbContext.Merchants.AnyAsync(
            m => m.Id != merchant.Id && m.NormalizedShopName == merchant.NormalizedShopName, ct);
        if (!shopNameTaken)
        {
            await outbox.PublishAsync(merchant.ToProfileChangedEvent());
            try
            {
                await outbox.SaveChangesAndFlushMessagesAsync(ct);
                await Send.OkAsync(merchant.ToResponse(), ct);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                AddError(MerchantUniqueness.ProfileChangedConcurrently);
                await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
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
