using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Model;

public record class Tag
{
    public Tag(TagEntity entity)
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        Name = entity.Name;
        ReleaseId = entity.ReleaseId;
        DateCreated = entity.DateCreated;
        InstallerDownloadUrl = entity.InstallerDownloadUrl;
        InstallerDownloads = entity.InstallerDownloads;
        IsPreRelease = entity.IsPreRelease;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public string Name { get; init; } = string.Empty;
    public long ReleaseId { get; init; }
    public DateTime DateCreated { get; init; }
    public string InstallerDownloadUrl { get; init; } = string.Empty;
    public int InstallerDownloads { get; init; }
    public bool IsPreRelease { get; init; }

    public TagEntity ToEntity() => new()
    {
        Id = Id,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        Name = Name,
        ReleaseId = ReleaseId,
        DateCreated = DateCreated,
        InstallerDownloadUrl = InstallerDownloadUrl,
        InstallerDownloads = InstallerDownloads,
        IsPreRelease = IsPreRelease,
    };
}

public record class TagGraph : Tag
{
    public TagGraph() : this(null!) { }
    public TagGraph(TagEntity tag, IEnumerable<TagAssetEntity> assets)
        : base(tag)
    {
        Assets = assets.Select(e => new TagAsset(e));
    }

    public IEnumerable<TagAsset> Assets { get; init; } = [];
}