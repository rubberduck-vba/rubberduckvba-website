using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.Api.Downloads;

public record class AvailableDownload
{
    public static AvailableDownload FromTag(Tag tag) => new AvailableDownload
    {
        Name = tag.Name,
        Title = $"Released {tag.DateCreated:yyyy-MM-dd}",
        Kind = tag.IsPreRelease ? "pre" : "tag",
        DownloadUrl = tag.InstallerDownloadUrl
    };

    public string Name { get; init; }
    public string Title { get; init; }
    public string DownloadUrl { get; init; }
    public string Kind { get; init; }
}
