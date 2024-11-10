namespace rubberduckvba.Server.Model.Entity;

public record class TagAssetEntity : Entity
{
    public int TagId { get; init; }
    public string DownloadUrl { get; init; } = default!;
}
