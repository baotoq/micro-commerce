using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace MicroCommerce.ApiService.Infrastructure;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<T?> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;
    Task RemoveAsync(string key);
}

public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await distributedCache.GetStringAsync(key);

        return value is null ? null : JsonSerializer.Deserialize<T>(value);
    }
    
    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl) where T : class
    {
        var cache = await GetAsync<T>(key);
        
        if (cache != null)
        {
            return cache;
        }
        
        var value = await factory();
        
        await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });
    }

    public async Task RemoveAsync(string key)
    {
        await distributedCache.RemoveAsync(key);
    }
}
