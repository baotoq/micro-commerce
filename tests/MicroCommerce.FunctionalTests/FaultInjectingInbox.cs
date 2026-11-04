using System.Collections.Concurrent;
using MicroCommerce.ApiService.Infrastructure.Messaging;

namespace MicroCommerce.FunctionalTests;

/// <summary>Messages whose next delivery should fail, to prove that a failed handler is retried.</summary>
public sealed class InboxFaults
{
    private readonly ConcurrentDictionary<string, byte> _failNext = new();

    public void FailNextDelivery(string messageId) => _failNext[messageId] = 0;

    public bool TryConsume(string messageId) => _failNext.TryRemove(messageId, out _);
}

/// <summary>The real inbox, except that it throws once for each message in <see cref="InboxFaults"/>.</summary>
public sealed class FaultInjectingInbox(Inbox inner, InboxFaults faults) : IInbox
{
    public Task<bool> TryRecordAsync(string consumer, string messageId, DateTimeOffset now, CancellationToken ct) =>
        faults.TryConsume(messageId)
            ? throw new InvalidOperationException("Injected inbox failure")
            : inner.TryRecordAsync(consumer, messageId, now, ct);
}
