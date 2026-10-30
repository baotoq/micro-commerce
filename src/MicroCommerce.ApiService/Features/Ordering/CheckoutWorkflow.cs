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
    public const string PaymentSucceededSignal = "PaymentSucceeded";
}

public record CheckoutPlaced(CheckoutId CheckoutId, DateTimeOffset ExpiresAt);

[Workflow]
public class CheckoutWorkflow
{
    private bool _paymentWindowElapsed;
    private bool _paymentSucceeded;

    [WorkflowRun]
    public async Task RunAsync(string checkoutId, DateTimeOffset expiresAt)
    {
        var remaining = expiresAt - Workflow.UtcNow;
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
        await Workflow.WaitConditionAsync(() => _paymentWindowElapsed || _paymentSucceeded, remaining);
        if (_paymentSucceeded)
        {
            await Workflow.ExecuteActivityAsync(
                (CheckoutActivities activities) => activities.MarkCheckoutPaidAsync(checkoutId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(1) });
        }
        else
        {
            await Workflow.ExecuteActivityAsync(
                (CheckoutActivities activities) => activities.ExpireCheckoutAsync(checkoutId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(1) });
        }
    }

    [WorkflowSignal(CheckoutTemporal.PaymentWindowElapsedSignal)]
    public Task PaymentWindowElapsedAsync()
    {
        _paymentWindowElapsed = true;
        return Task.CompletedTask;
    }

    [WorkflowSignal(CheckoutTemporal.PaymentSucceededSignal)]
    public Task PaymentSucceededAsync()
    {
        _paymentSucceeded = true;
        return Task.CompletedTask;
    }
}

public sealed class CheckoutActivities(OrderingDbContext db, ICatalogReservations reservations)
{
    [Activity]
    public async Task MarkCheckoutPaidAsync(string checkoutId)
    {
        var id = CheckoutId.From(Guid.Parse(checkoutId));
        var ct = ActivityExecutionContext.Current.CancellationToken;
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var updated = await db.Checkouts
                .Where(c => c.Id == id && c.Status == CheckoutStatus.AwaitingPayment)
                .ExecuteUpdateAsync(update => update.SetProperty(c => c.Status, CheckoutStatus.Paid), ct);
            if (updated == 0)
            {
                await transaction.RollbackAsync(ct);
                return;
            }

            await reservations.CommitAsync(id, ct);
            var groups = await db.CheckoutMerchants.Where(group => group.CheckoutId == id).ToListAsync(ct);
            db.Orders.AddRange(groups.Select(Order.FromGroup));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });
    }

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
