using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Model;

public record class TagAsset()
{
    public TagAsset(TagAssetEntity entity) : this()
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        Name = entity.Name;
        TagId = entity.TagId;
        DownloadUrl = entity.DownloadUrl;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public string Name { get; init; } = string.Empty;
    public int TagId { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;

    public TagAssetEntity ToEntity() => new()
    {
        Id = Id,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        Name = Name,
        DownloadUrl = DownloadUrl,
        TagId = TagId
    };
}