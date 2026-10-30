using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Temporalio.Client;

namespace MicroCommerce.ApiService.Features.Ordering;

public record PayCheckoutRequest(CheckoutId Id);

public class PayCheckoutEndpoint(
    OrderingDbContext db,
    CurrentAccount currentAccount,
    ITemporalClient temporal) : Endpoint<PayCheckoutRequest>
{
    public override void Configure()
    {
        Post("/dev/checkouts/{id:guid}/pay");
    }

    public override async Task HandleAsync(PayCheckoutRequest request, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var checkout = await db.Checkouts.AsNoTracking()
            .Where(c => c.Id == request.Id && c.AccountId == account.Id)
            .Select(c => new { c.Id, c.ExpiresAt, c.Status })
            .SingleOrDefaultAsync(ct);
        if (checkout is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (checkout.Status == CheckoutStatus.Expired)
        {
            AddError("An Expired Checkout cannot be paid.");
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return;
        }

        if (checkout.Status == CheckoutStatus.AwaitingPayment)
        {
            var id = checkout.Id.Value.ToString();
            await temporal.StartWorkflowAsync(
                (CheckoutWorkflow workflow) => workflow.RunAsync(id, checkout.ExpiresAt),
                new(id: id, taskQueue: CheckoutTemporal.TaskQueue)
                {
                    IdConflictPolicy = Temporalio.Api.Enums.V1.WorkflowIdConflictPolicy.UseExisting,
                    StartSignal = CheckoutTemporal.PaymentSucceededSignal,
                    StartSignalArgs = [],
                });
        }

        await Send.StatusCodeAsync(StatusCodes.Status202Accepted, ct);
    }
}
