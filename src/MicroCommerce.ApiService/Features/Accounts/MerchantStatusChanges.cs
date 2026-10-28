using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public abstract record MerchantStatusChangeResult
{
    public sealed record NotFound : MerchantStatusChangeResult;
    public sealed record Conflict : MerchantStatusChangeResult;
    public sealed record Succeeded(Merchant Merchant) : MerchantStatusChangeResult;
}

/// <summary>Changes a Merchant's selling status and publishes the change in the Accounts outbox.</summary>
public sealed class MerchantStatusChanges(IDbContextOutbox<AccountsDbContext> outbox)
{
    public Task<MerchantStatusChangeResult> SuspendAsync(MerchantId id, CancellationToken ct) =>
        ChangeAsync(id, MerchantStatus.Suspended, ct);

    public Task<MerchantStatusChangeResult> ReinstateAsync(MerchantId id, CancellationToken ct) =>
        ChangeAsync(id, MerchantStatus.Active, ct);

    private async Task<MerchantStatusChangeResult> ChangeAsync(MerchantId id, MerchantStatus status, CancellationToken ct)
    {
        var merchant = await outbox.DbContext.Merchants.SingleOrDefaultAsync(m => m.Id == id, ct);
        if (merchant is null) return new MerchantStatusChangeResult.NotFound();

        var changed = status switch
        {
            MerchantStatus.Suspended => merchant.Suspend(),
            MerchantStatus.Active => merchant.Reinstate(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
        if (!changed) return new MerchantStatusChangeResult.Succeeded(merchant);

        switch (status)
        {
            case MerchantStatus.Suspended:
                await outbox.PublishAsync(merchant.ToSuspendedEvent());
                break;
            case MerchantStatus.Active:
                await outbox.PublishAsync(merchant.ToReinstatedEvent());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        try
        {
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new MerchantStatusChangeResult.Conflict();
        }

        return new MerchantStatusChangeResult.Succeeded(merchant);
    }
}
