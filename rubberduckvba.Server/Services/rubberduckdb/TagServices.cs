using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Services.rubberduckdb;

public class TagServices(IRepository<TagEntity> tagsRepository, IRepository<TagAssetEntity> tagAssetsRepository)
{
    private IEnumerable<TagEntity> _allTags = [];
    private IEnumerable<TagEntity> _latestTags = [];
    private TagGraph? _main;
    private TagGraph? _next;
    private bool _mustInvalidate = true;

    public IEnumerable<Tag> GetAllTags()
    {
        if (_mustInvalidate || !_allTags.Any())
        {
            _allTags = tagsRepository.GetAll().ToList();
            _latestTags = _allTags
                .GroupBy(tag => tag.IsPreRelease)
                .Select(tags => tags.OrderByDescending(tag => tag.DateCreated))
                .SelectMany(tags => tags.Take(1))
                .ToList();
            _mustInvalidate = false;
        }

        return _allTags.Select(e => new Tag(e));
    }

    public IEnumerable<Tag> GetLatestTags()
    {
        if (_mustInvalidate || !_latestTags.Any())
        {
            _ = GetAllTags();
        }

        return _latestTags.Select(e => new Tag(e));
    }

    public TagGraph GetLatestTag(bool isPreRelease)
    {
        var mustInvalidate = _mustInvalidate;
        if (mustInvalidate || !_latestTags.Any())
        {
            _ = GetAllTags(); // _mustInvalidate => false
        }

        if (!mustInvalidate && !isPreRelease && _main != null)
        {
            return _main;
        }
        if (!mustInvalidate && isPreRelease && _next != null)
        {
            return _next;
        }

        var mainTag = _latestTags.First(e => !e.IsPreRelease);
        var mainAssets = tagAssetsRepository.GetAll(mainTag.Id);
        _main = new TagGraph(mainTag, mainAssets);

        var nextTag = _latestTags.First(e => e.IsPreRelease);
        var nextAssets = tagAssetsRepository.GetAll(nextTag.Id);
        _next = new TagGraph(nextTag, nextAssets);

        return isPreRelease ? _next : _main;
    }

    public void Create(IEnumerable<TagGraph> tags)
    {
        var tagEntities = tagsRepository.Insert(tags.Select(tag => tag.ToEntity()));
        var tagsByName = tagEntities.ToDictionary(
            tag => tag.Name,
            tag => new TagGraph(tag, tags.Single(t => t.Name == tag.Name).Assets.Select(a => a.ToEntity())));

        var assets = new List<TagAssetEntity>();
        foreach (var tagEntity in tagEntities)
        {
            var tag = tagsByName[tagEntity.Name];
            assets.AddRange(tag.Assets.Select(asset => (asset with { TagId = tag.Id }).ToEntity()));
        }

        _ = tagAssetsRepository.Insert(assets);
        _mustInvalidate = true;
    }

    public void Update(IEnumerable<Tag> tags)
    {
        tagsRepository.Update(tags.Select(tag => tag.ToEntity()));
        _mustInvalidate = true;
    }
}