using MicroCommerce.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Infrastructure.Messaging;

/// <summary>
/// De-duplicates deliveries: each (consumer, message) pair is recorded once, in the same SaveChanges
/// as the handler's effect.
/// </summary>
public sealed class Inbox(AppDbContext db)
{
    /// <summary>Returns false when this consumer has already handled the message.</summary>
    public async Task<bool> TryRecordAsync(string consumer, string messageId, DateTimeOffset now, CancellationToken ct)
    {
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
