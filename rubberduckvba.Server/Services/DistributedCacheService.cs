using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace rubberduckvba.Server.Services;

public interface ICacheService
{
    bool TryGet<T>(string key, out T value);
    void Write<T>(string key, T value);

    void Invalidate(string key);
    void Invalidate();
}

public class CacheService(IDistributedCache cache) : ICacheService
{
    private static DistributedCacheEntryOptions CacheOptions { get; } = new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromHours(1),
    };

    private readonly ConcurrentDictionary<string, DateTime> _keys = [];

    public void Invalidate()
    {
        foreach (var key in _keys.Keys)
        {
            Invalidate(key);
        }
    }

    public void Invalidate(string key)
    {
        _keys.TryRemove(key, out _);
        cache.Remove(key);
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (!_keys.TryGetValue(key, out _))
        {
            value = default!;
            return false;
        }

        var bytes = cache.Get(key);
        if (bytes == null)
        {
            Invalidate(key);
            value = default!;
            return false;
        }

        _keys[key] = TimeProvider.System.GetUtcNow().DateTime;
        value = JsonSerializer.Deserialize<T>(bytes)!;
        return value != null;
    }

    public void Write<T>(string key, T value)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        cache.Set(key, bytes, CacheOptions);
        _keys[key] = TimeProvider.System.GetUtcNow().DateTime;
    }
}
