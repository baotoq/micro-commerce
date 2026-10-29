using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Temporalio.Client;

namespace MicroCommerce.ApiService.Features.Ordering;

public record ExpireCheckoutRequest(CheckoutId Id);

public class ExpireCheckoutEndpoint(
    OrderingDbContext db,
    CurrentAccount currentAccount,
    ITemporalClient temporal) : Endpoint<ExpireCheckoutRequest>
{
    public override void Configure()
    {
        Post("/dev/checkouts/{id:guid}/expire");
    }

    public override async Task HandleAsync(ExpireCheckoutRequest request, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var checkout = await db.Checkouts.AsNoTracking()
            .Where(c => c.Id == request.Id && c.AccountId == account.Id)
            .Select(c => new { c.Id, c.ExpiresAt })
            .SingleOrDefaultAsync(ct);
        if (checkout is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var id = checkout.Id.Value.ToString();
        await temporal.StartWorkflowAsync(
            (CheckoutWorkflow workflow) => workflow.RunAsync(id, checkout.ExpiresAt),
            new(id: id, taskQueue: CheckoutTemporal.TaskQueue)
            {
                IdConflictPolicy = Temporalio.Api.Enums.V1.WorkflowIdConflictPolicy.UseExisting,
                StartSignal = CheckoutTemporal.PaymentWindowElapsedSignal,
                StartSignalArgs = [],
            });
        await Send.StatusCodeAsync(StatusCodes.Status202Accepted, ct);
    }
}
