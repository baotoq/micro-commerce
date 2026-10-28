using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

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

public abstract class MerchantStatusChangeEndpoint : Endpoint<ChangeMerchantStatusRequest, OperatorMerchantResponse>
{
    private const string ConcurrencyMessage = "Merchant status changed concurrently; try again.";

    protected abstract Task<MerchantStatusChangeResult> ChangeStatusAsync(MerchantId id, CancellationToken ct);

    public override async Task HandleAsync(ChangeMerchantStatusRequest req, CancellationToken ct)
    {
        switch (await ChangeStatusAsync(req.Id, ct))
        {
            case MerchantStatusChangeResult.NotFound:
                await Send.NotFoundAsync(ct);
                break;
            case MerchantStatusChangeResult.Conflict:
                AddError(ConcurrencyMessage);
                await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
                break;
            case MerchantStatusChangeResult.Succeeded success:
                await Send.OkAsync(success.Merchant.ToOperatorResponse(), ct);
                break;
            default:
                throw new InvalidOperationException($"Unknown {nameof(MerchantStatusChangeResult)} result.");
        }
    }
}

/// <summary>Suspends a Merchant and publishes its status through the Accounts outbox.</summary>
public sealed class SuspendMerchantEndpoint(MerchantStatusChanges changes) : MerchantStatusChangeEndpoint
{
    public override void Configure()
    {
        Post("/merchants/{id:guid}/suspend");
        Group<PlatformOperations>();
    }

    protected override Task<MerchantStatusChangeResult> ChangeStatusAsync(MerchantId id, CancellationToken ct) =>
        changes.SuspendAsync(id, ct);
}

/// <summary>Reinstates a Merchant and publishes its status through the Accounts outbox.</summary>
public sealed class ReinstateMerchantEndpoint(MerchantStatusChanges changes) : MerchantStatusChangeEndpoint
{
    public override void Configure()
    {
        Post("/merchants/{id:guid}/reinstate");
        Group<PlatformOperations>();
    }

    protected override Task<MerchantStatusChangeResult> ChangeStatusAsync(MerchantId id, CancellationToken ct) =>
        changes.ReinstateAsync(id, ct);
}
