using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Services.rubberduckdb;

public class TagServices(IRepository<TagEntity> tagsRepository, IRepository<TagAssetEntity> tagAssetsRepository)
{
    public bool TryGetTag(string name, out Tag tag)
    {
        var entity = tagsRepository.GetAll().SingleOrDefault(tag => tag.Name == name);
        if (entity is null)
        {
            tag = default!;
            return false;
        }

        tag = new Tag(entity);
        return true;
    }

    public IEnumerable<Tag> GetAllTags()
    {
        return tagsRepository.GetAll().Select(e => new Tag(e));
    }

    public IEnumerable<Tag> GetLatestTags() => GetLatestTags(tagsRepository.GetAll().Select(e => new Tag(e)));

    public IEnumerable<Tag> GetLatestTags(IEnumerable<Tag> allTags) => allTags
        .GroupBy(tag => tag.IsPreRelease)
        .Select(tags => tags.OrderByDescending(tag => tag.DateCreated))
        .SelectMany(tags => tags.Take(1))
        .ToList();

    public TagGraph GetLatestTag(bool isPreRelease)
    {
        var latestTags = GetLatestTags();

        if (!isPreRelease)
        {
            var mainTag = latestTags.First(e => !e.IsPreRelease);
            var mainAssets = tagAssetsRepository.GetAll(mainTag.Id);
            return new TagGraph(mainTag.ToEntity(), mainAssets);
        }
        else
        {
            var nextTag = latestTags.First(e => e.IsPreRelease);
            var nextAssets = tagAssetsRepository.GetAll(nextTag.Id);
            return new TagGraph(nextTag.ToEntity(), nextAssets);
        }
    }

    public void Create(IEnumerable<TagGraph> tags)
    {
        if (!tags.Any())
        {
            return;
        }

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
    }

    public void Update(IEnumerable<Tag> tags)
    {
        if (!tags.Any())
        {
            return;
        }

        tagsRepository.Update(tags.Select(tag => tag.ToEntity()));
    }
}