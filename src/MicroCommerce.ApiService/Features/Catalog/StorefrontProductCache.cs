using System.Text.Json;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// Storefront Product details in Redis. Whatever changes what a Buyer sees of a Product evicts it: editing,
/// publishing or unpublishing the Product, a change to its Shop, or a Reserve, Commit or Release of its stock. Entries
/// also expire, which bounds how long a read that raced an eviction can serve what it read before the change, and how
/// long a hold that lapses by its deadline (which raises nothing) can lag.
/// </summary>
public class StorefrontProductCache(IDistributedCache cache)
{
    private static readonly DistributedCacheEntryOptions EntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
    };

    private static string Key(ProductId id) => $"storefront:product:{id}";

    public async Task<StorefrontProductResponse?> GetAsync(ProductId id, CancellationToken ct) =>
        await cache.GetAsync(Key(id), ct) is { } cached
            ? JsonSerializer.Deserialize<StorefrontProductResponse>(cached)
            : null;

    public Task SetAsync(StorefrontProductResponse product, CancellationToken ct) =>
        cache.SetAsync(Key(product.Id), JsonSerializer.SerializeToUtf8Bytes(product), EntryOptions, ct);

    public Task EvictAsync(ProductId id, CancellationToken ct) => cache.RemoveAsync(Key(id), ct);

    public async Task EvictAsync(IEnumerable<ProductId> ids, CancellationToken ct)
    {
        foreach (var id in ids) await EvictAsync(id, ct);
    }
}
