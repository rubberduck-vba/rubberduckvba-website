using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace rubberduckvba.Server.Services;

public interface IContentCacheService : IDisposable
{
    int Count { get; }
    void Clear();
    void Invalidate(string key, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase);
    void Invalidate(Func<string, bool> predicate);
    void SetValue<T>(string key, T value);
    bool TryGetValue<T>(string key, out T value);
}

public class ContentCacheService : IContentCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, string> _cacheKeys = new ConcurrentDictionary<string, string>();

    private static readonly MemoryCacheEntryOptions _cacheOptions =
        new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        };

    public ContentCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public int Count => _cacheKeys.Count;

    public void Clear()
    {
        foreach (var key in _cacheKeys)
        {
            _cache.Remove(key);
        }
        _cacheKeys.Clear();
        Debug.WriteLine("**Cache cleared");
    }

    public void Invalidate(string key, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase) =>
        Invalidate((e) => key.IndexOf(e, stringComparison) >= 0);

    public void Invalidate(Func<string, bool> predicate)
    {
        var keys = _cacheKeys.Keys.Where(predicate).ToArray();
        foreach (var key in keys)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
    }

    public void SetValue<T>(string key, T value)
    {
        _cache.Set(key, value, _cacheOptions);
        _cacheKeys.TryAdd(key, key);
        //Debug.WriteLine($"**Cache WRITE for key: '{key}'");
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        if (_cache.TryGetValue(key, out value))
        {
            //Debug.WriteLine($"**Cache READ for key: '{key}'");
            return true;
        }

        //Debug.WriteLine($"**Cache MISS for key: '{key}'");
        _cacheKeys.TryRemove(key, out _);
        return false;
    }

    public void Dispose()
    {
        Clear();
        _cache.Dispose();
        //Debug.WriteLine("**Cache disposed");
    }
}
