using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Extensions.Hosting;
using Temporalio.Workflows;

namespace MicroCommerce.ApiService.Features.Ordering;

public static class CheckoutTemporal
{
    public const string TaskQueue = "ordering";
    public const string PaymentWindowElapsedSignal = "PaymentWindowElapsed";
}

public record CheckoutPlaced(CheckoutId CheckoutId, DateTimeOffset ExpiresAt);

[Workflow]
public class CheckoutWorkflow
{
    private bool _paymentWindowElapsed;

    [WorkflowRun]
    public async Task RunAsync(string checkoutId, DateTimeOffset expiresAt)
    {
        var remaining = expiresAt - Workflow.UtcNow;
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
        await Workflow.WaitConditionAsync(() => _paymentWindowElapsed, remaining);
        await Workflow.ExecuteActivityAsync(
            (CheckoutActivities activities) => activities.ExpireCheckoutAsync(checkoutId),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(1) });
    }

    [WorkflowSignal(CheckoutTemporal.PaymentWindowElapsedSignal)]
    public Task PaymentWindowElapsedAsync()
    {
        _paymentWindowElapsed = true;
        return Task.CompletedTask;
    }
}

public sealed class CheckoutActivities(OrderingDbContext db, ICatalogReservations reservations)
{
    [Activity]
    public async Task ExpireCheckoutAsync(string checkoutId)
    {
        var id = CheckoutId.From(Guid.Parse(checkoutId));
        var ct = ActivityExecutionContext.Current.CancellationToken;
        if (!await db.Checkouts.AnyAsync(
                c => c.Id == id && c.Status == CheckoutStatus.AwaitingPayment, ct))
            return;

        var expired = await db.Checkouts
            .Where(c => c.Id == id && c.Status == CheckoutStatus.AwaitingPayment)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.Status, CheckoutStatus.Expired), ct);
        if (expired == 1) await reservations.ReleaseAsync(id, ct);
    }
}

public static class CheckoutPlacedHandler
{
    public static async Task Handle(CheckoutPlaced message, ITemporalClient temporal)
    {
        await temporal.StartWorkflowAsync(
            (CheckoutWorkflow workflow) => workflow.RunAsync(message.CheckoutId.Value.ToString(), message.ExpiresAt),
            new(id: message.CheckoutId.Value.ToString(), taskQueue: CheckoutTemporal.TaskQueue)
            {
                IdConflictPolicy = Temporalio.Api.Enums.V1.WorkflowIdConflictPolicy.UseExisting,
            });
    }
}
