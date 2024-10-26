using Microsoft.Extensions.Caching.Distributed;
using NLog.Targets;
using System.Text;
using System.Text.Json;

namespace rubberduckvba.com.Server.Services;

public interface ICacheService
{
    bool TryGet<T>(string key, out T value);
    void Write<T>(string key, T value);

    void Invalidate(string key);
}

public class CacheService(IDistributedCache cache) : ICacheService
{
    private static DistributedCacheEntryOptions CacheOptions { get; } = new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromHours(24),
    };

    public void Invalidate(string key)
    {
        cache.Remove(key);
    }

    public bool TryGet<T>(string key, out T value)
    {
        var bytes = cache.Get(key);
        if (bytes == null)
        {
            value = default!;
            return false;
        }

        value = JsonSerializer.Deserialize<T>(bytes)!;
        return value != null;
    }

    public void Write<T>(string key, T value)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        cache.Set(key, bytes, CacheOptions);
    }
}
