using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Model.Entity;

public record class TagEntity : Entity
{
    public RepositoryId RepositoryId { get; init; } = RepositoryId.Rubberduck;
    public long ReleaseId { get; init; }
    public DateTime DateCreated { get; init; }
    public string InstallerDownloadUrl { get; init; } = default!;
    public int InstallerDownloads { get; init; }
    public bool IsPreRelease { get; init; }
}
