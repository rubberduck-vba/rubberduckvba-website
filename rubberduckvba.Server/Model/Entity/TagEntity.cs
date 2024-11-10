namespace rubberduckvba.Server.Model.Entity;

public record class TagEntity : Entity
{
    public long ReleaseId { get; init; }
    public DateTime DateCreated { get; init; }
    public string InstallerDownloadUrl { get; init; } = default!;
    public int InstallerDownloads { get; init; }
    public bool IsPreRelease { get; init; }
}
