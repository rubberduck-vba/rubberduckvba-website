using Microsoft.Extensions.Caching.Memory;
using rubberduckvba.Server.Api.Downloads;
using rubberduckvba.Server.Api.Features;
using rubberduckvba.Server.Api.Tags;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using System.Security.Cryptography;
using System.Text;

namespace rubberduckvba.Server.Services;

public class CacheService
{
    private readonly MemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;

    private readonly HangfireJobStateRepository _repository;
    private readonly ILogger _logger;

    private const string JobStateSucceeded = "Succeeded";
    private const string TagsJobName = "update_installer_downloads";
    private const string XmldocJobName = "update_xmldoc_content";

    public CacheService(HangfireJobStateRepository repository, ILogger<CacheService> logger)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(1),
        };

        _repository = repository;
        _logger = logger;
    }

    public HangfireJobState? TagsJobState { get; private set; }
    public HangfireJobState? XmldocJobState { get; private set; }

    private void GetCurrentJobState()
    {
        var state = _repository.GetAll().ToDictionary(e => e.JobName);
        TagsJobState = state.TryGetValue(TagsJobName, out var tagsJobState) ? tagsJobState : null;
        XmldocJobState = state.TryGetValue(XmldocJobName, out var xmldocsJobState) ? xmldocsJobState : null;
    }

    public bool TryGetHtml(string markdown, out string? cached) => TryReadFromCache($"md:{Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(markdown)))}", out cached);
    public void CacheHtml(string markdown, string html) => Write($"md:{Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(markdown)))}", html);

    public bool TryGetLatestTags(out LatestTagsViewModel? cached) => TryReadFromTagsCache("tags/latest", out cached);
    public bool TryGetAvailableDownloads(out AvailableDownload[]? cached) => TryReadFromTagsCache("downloads", out cached);
    public bool TryGetFeatures(out FeatureViewModel[]? cached) => TryReadXmldocCache("features", out cached);
    public bool TryGetFeature(string name, out FeatureViewModel? cached) => TryReadXmldocCache($"features/{name.ToLowerInvariant()}", out cached);
    public bool TryGetInspections(out InspectionsFeatureViewModel? cached) => TryReadXmldocCache($"inspections", out cached);
    public bool TryGetInspection(string name, out InspectionViewModel? cached) => TryReadXmldocCache($"inspections/{name.ToLowerInvariant()}", out cached);
    public bool TryGetQuickFixes(out QuickFixesFeatureViewModel? cached) => TryReadXmldocCache($"quickfixes", out cached);
    public bool TryGetQuickFix(string name, out QuickFixViewModel? cached) => TryReadXmldocCache($"quickfixes/{name.ToLowerInvariant()}", out cached);
    public bool TryGetAnnotations(out AnnotationsFeatureViewModel? cached) => TryReadXmldocCache($"annotations", out cached);
    public bool TryGetAnnotation(string name, out AnnotationViewModel? cached) => TryReadXmldocCache($"annotations/{name.ToLowerInvariant()}", out cached);

    public void Invalidate(LatestTagsViewModel newContent)
    {
        GetCurrentJobState();
        if (TagsJobState?.StateName == JobStateSucceeded)
        {
            Write("tags/latest", newContent);
        }
        else
        {
            _logger.LogWarning($"TagsJobState is not '{JobStateSucceeded}' as expected (LastJobId: {TagsJobState?.LastJobId}); content will not be cached");
        }
    }

    public void Invalidate(AvailableDownload[] newContent)
    {
        GetCurrentJobState();
        if (TagsJobState?.StateName == JobStateSucceeded)
        {
            Write("downloads", newContent);
        }
        else
        {
            _logger.LogWarning($"TagsJobState is not '{JobStateSucceeded}' as expected (LastJobId: {TagsJobState?.LastJobId}); content will not be cached");
        }
    }
    public void Invalidate(FeatureViewModel newContent)
    {
        GetCurrentJobState();
        if (XmldocJobState?.StateName == JobStateSucceeded)
        {
            Write($"features/{newContent.Name.ToLowerInvariant()}", newContent);
        }
        else
        {
            _logger.LogWarning($"XmldocJobState is not '{JobStateSucceeded}' as expected (LastJobId: {XmldocJobState?.LastJobId}); content will not be cached");
        }
    }
    public void Invalidate(FeatureViewModel[] newContent)
    {
        GetCurrentJobState();
        if (XmldocJobState?.StateName == JobStateSucceeded)
        {
            Write($"features", newContent);
        }
        else
        {
            _logger.LogWarning($"XmldocJobState is not '{JobStateSucceeded}' as expected (LastJobId: {XmldocJobState?.LastJobId}); content will not be cached");
        }
    }
    public void Invalidate(InspectionsFeatureViewModel newContent)
    {
        GetCurrentJobState();
        if (!TryReadXmldocCache<InspectionsFeatureViewModel>("inspections", out _) || XmldocJobState?.StateName == JobStateSucceeded)
        {
            Write("inspections", newContent);
            foreach (var item in newContent.Inspections)
            {
                Write($"inspections/{item.Name.ToLowerInvariant()}", item);
            }
        }
        else
        {
            _logger.LogWarning($"XmldocJobState is not '{JobStateSucceeded}' as expected (LastJobId: {XmldocJobState?.LastJobId}); content will not be cached");
        }
    }
    public void Invalidate(QuickFixesFeatureViewModel newContent)
    {
        GetCurrentJobState();
        if (!TryReadXmldocCache<QuickFixesFeatureViewModel>("quickfixes", out _) || XmldocJobState?.StateName == JobStateSucceeded)
        {
            Write("quickfixes", newContent);
            foreach (var item in newContent.QuickFixes)
            {
                Write($"quickfixes/{item.Name.ToLowerInvariant()}", item);
            }
        }
        else
        {
            _logger.LogWarning($"XmldocJobState is not '{JobStateSucceeded}' as expected (LastJobId: {XmldocJobState?.LastJobId}); content will not be cached");
        }
    }
    public void Invalidate(AnnotationsFeatureViewModel newContent)
    {
        GetCurrentJobState();
        if (!TryReadXmldocCache<AnnotationsFeatureViewModel>("annotations", out _) || XmldocJobState?.StateName == JobStateSucceeded)
        {
            Write("annotations", newContent);
            foreach (var item in newContent.Annotations)
            {
                Write($"annotations/{item.Name.ToLowerInvariant()}", item);
            }
        }
        else
        {
            _logger.LogWarning($"XmldocJobState is not '{JobStateSucceeded}' as expected (LastJobId: {XmldocJobState?.LastJobId}); content will not be cached");
        }
    }

    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache was cleared.");
    }

    private void Write<T>(string key, T value)
    {
        _cache.Set(key, value, _options);
        _logger.LogInformation("Cache key '{key}' was invalidated.", key);
    }

    private bool TryReadFromTagsCache<T>(string key, out T? cached)
    {
        var result = _cache.TryGetValue(key, out cached);
        _logger.LogDebug("TagsCache hit: '{key}' (valid: {result})", key, result);

        return result;
    }

    private bool TryReadXmldocCache<T>(string key, out T? cached)
    {
        var result = _cache.TryGetValue(key, out cached);
        _logger.LogDebug("XmldocCache hit: '{key}' (valid: {result})", key, result);

        return result;
    }

    private bool TryReadFromCache(string key, out string? cached)
    {
        var result = _cache.TryGetValue(key, out cached);
        return result;
    }
}