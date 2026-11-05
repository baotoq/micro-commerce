using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>A Merchant as other areas see it.</summary>
public sealed record MerchantRef(MerchantId Id, string ShopName);

/// <summary>A Merchant's public Shop facts. Never the Pickup Address (FR-031).</summary>
public sealed record ShopRef(MerchantId Id, string ShopName, string? Description);

/// <summary>
/// The contract Accounts publishes so other areas (Catalog) learn about Merchants without touching
/// Accounts data (Principle II, research R1).
/// </summary>
public interface IMerchantDirectory
{
    Task<MerchantRef?> FindByAccountAsync(AccountId accountId, CancellationToken ct);

    Task<IReadOnlyDictionary<MerchantId, ShopRef>> GetShopsAsync(IReadOnlyCollection<MerchantId> ids, CancellationToken ct);
}

internal sealed class EfMerchantDirectory(AccountsDbContext db) : IMerchantDirectory
{
    public Task<MerchantRef?> FindByAccountAsync(AccountId accountId, CancellationToken ct) =>
        db.Merchants.AsNoTracking()
            .Where(m => m.AccountId == accountId)
            .Select(m => new MerchantRef(m.Id, m.ShopName.Value))
            .SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<MerchantId, ShopRef>> GetShopsAsync(IReadOnlyCollection<MerchantId> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<MerchantId, ShopRef>();

        var merchants = await db.Merchants.AsNoTracking().Where(m => ids.Contains(m.Id)).ToListAsync(ct);
        return merchants.ToDictionary(m => m.Id, m => new ShopRef(m.Id, m.ShopName.Value, m.Description));
    }
}
