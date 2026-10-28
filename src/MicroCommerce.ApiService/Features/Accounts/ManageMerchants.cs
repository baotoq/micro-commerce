using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public record OperatorMerchantResponse(MerchantId Id, ShopName ShopName, MerchantStatus Status, DateTimeOffset CreatedAt);

public record ChangeMerchantStatusRequest(MerchantId Id);

/// <summary>Every Merchant and its selling status, for a Platform Operator.</summary>
public class ListMerchantsEndpoint(AccountsDbContext db) : EndpointWithoutRequest<IReadOnlyList<OperatorMerchantResponse>>
{
    public override void Configure()
    {
        Get("/merchants");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchants = await db.Merchants
            .OrderBy(m => m.CreatedAt)
            .ProjectToOperatorResponse()
            .ToListAsync(ct);

        await Send.OkAsync(merchants, ct);
    }
}

/// <summary>Suspends a Merchant and publishes its status through the Accounts outbox.</summary>
public class SuspendMerchantEndpoint(IDbContextOutbox<AccountsDbContext> outbox)
    : Endpoint<ChangeMerchantStatusRequest, OperatorMerchantResponse>
{
    public override void Configure()
    {
        Post("/merchants/{id:guid}/suspend");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(ChangeMerchantStatusRequest req, CancellationToken ct)
    {
        var merchant = await outbox.DbContext.Merchants.SingleOrDefaultAsync(m => m.Id == req.Id, ct);
        if (merchant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!merchant.Suspend())
        {
            await Send.OkAsync(merchant.ToOperatorResponse(), ct);
            return;
        }

        await outbox.PublishAsync(merchant.ToSuspendedEvent());
        if (!await SaveStatusAsync(ct)) return;
        await Send.OkAsync(merchant.ToOperatorResponse(), ct);
    }

    private async Task<bool> SaveStatusAsync(CancellationToken ct)
    {
        try
        {
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            AddError("Merchant status changed concurrently; try again.");
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return false;
        }
    }
}

/// <summary>Reinstates a Merchant and publishes its status through the Accounts outbox.</summary>
public class ReinstateMerchantEndpoint(IDbContextOutbox<AccountsDbContext> outbox)
    : Endpoint<ChangeMerchantStatusRequest, OperatorMerchantResponse>
{
    public override void Configure()
    {
        Post("/merchants/{id:guid}/reinstate");
        Group<PlatformOperations>();
    }

    public override async Task HandleAsync(ChangeMerchantStatusRequest req, CancellationToken ct)
    {
        var merchant = await outbox.DbContext.Merchants.SingleOrDefaultAsync(m => m.Id == req.Id, ct);
        if (merchant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!merchant.Reinstate())
        {
            await Send.OkAsync(merchant.ToOperatorResponse(), ct);
            return;
        }

        await outbox.PublishAsync(merchant.ToReinstatedEvent());
        if (!await SaveStatusAsync(ct)) return;
        await Send.OkAsync(merchant.ToOperatorResponse(), ct);
    }

    private async Task<bool> SaveStatusAsync(CancellationToken ct)
    {
        try
        {
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            AddError("Merchant status changed concurrently; try again.");
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return false;
        }
    }
}
