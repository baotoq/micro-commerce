using System.Security.Claims;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// Maps a signed-in token subject to its Account, creating the Account on the subject's first request.
/// </summary>
public class CurrentAccount(AccountsDbContext db)
{
    public async Task<Account> GetOrCreateAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var subject = user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("The authenticated token has no subject.");

        var existing = await FindAsync(subject, ct);
        if (existing is not null) return existing;

        var email = user.FindFirstValue("email");
        var account = new Account
        {
            Subject = subject,
            Email = email,
            DisplayName = user.FindFirstValue("name") ?? user.FindFirstValue("preferred_username") ?? email ?? subject,
        };
        db.Accounts.Add(account);

        try
        {
            await db.SaveChangesAsync(ct);
            return account;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // A concurrent first request for the same subject created it first.
            db.Entry(account).State = EntityState.Detached;
            return await FindAsync(subject, ct)
                ?? throw new InvalidOperationException($"Account for subject {subject} vanished after a conflict.");
        }
    }

    private Task<Account?> FindAsync(string subject, CancellationToken ct) =>
        db.Accounts.AsNoTracking().SingleOrDefaultAsync(a => a.Subject == subject, ct);
}
