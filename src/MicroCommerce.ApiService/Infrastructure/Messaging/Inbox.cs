using MicroCommerce.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Infrastructure.Messaging;

public interface IInbox
{
    /// <summary>Returns false when this consumer has already handled the message.</summary>
    Task<bool> TryRecordAsync(string consumer, string messageId, DateTimeOffset now, CancellationToken ct);
}

/// <summary>
/// De-duplicates deliveries: each (consumer, message) pair is recorded once, in the same SaveChanges
/// as the handler's effect.
/// </summary>
public sealed class Inbox(AppDbContext db) : IInbox
{
    public async Task<bool> TryRecordAsync(string consumer, string messageId, DateTimeOffset now, CancellationToken ct)
    {
        // Duplicates are expected (at-least-once delivery): check first, so a redelivery doesn't
        // surface as a failed insert in the logs. The key still catches two deliveries racing.
        if (await db.ProcessedEvents.AnyAsync(e => e.Consumer == consumer && e.MessageId == messageId, ct)) return false;

        var entry = db.ProcessedEvents.Add(new ProcessedEvent { Consumer = consumer, MessageId = messageId, ProcessedAt = now });
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException e) when (e.IsUniqueViolation())
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }
}
