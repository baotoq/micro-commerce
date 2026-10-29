using Npgsql;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// For concurrency tests that hold a row lock so concurrent requests queue up behind it: how many are waiting. Only
/// waits on the test's own lock count, so other tests sharing the database can't make it look ready.
/// </summary>
public static class LockWaits
{
    /// <summary>
    /// The sessions blocked by <paramref name="holder"/>'s locks, directly or behind another session it blocks (a
    /// second request queues on the first one's lock, not the holder's). Read on a fresh connection: within the
    /// holder's own transaction, the activity view keeps the snapshot it first took.
    /// </summary>
    public static async Task<long> CountBlockedByAsync(string connectionString, NpgsqlConnection holder, CancellationToken ct)
    {
        await using var probe = new NpgsqlConnection(connectionString);
        await probe.OpenAsync(ct);
        await using var count = new NpgsqlCommand(
            """
            WITH direct AS (SELECT pid FROM pg_stat_activity WHERE @holder = ANY(pg_blocking_pids(pid)))
            SELECT count(*) FROM pg_stat_activity a
            WHERE a.pid IN (SELECT pid FROM direct)
               OR EXISTS (SELECT 1 FROM direct d WHERE d.pid = ANY(pg_blocking_pids(a.pid)))
            """, probe);
        count.Parameters.AddWithValue("holder", holder.ProcessID);
        return (long)(await count.ExecuteScalarAsync(ct))!;
    }
}
