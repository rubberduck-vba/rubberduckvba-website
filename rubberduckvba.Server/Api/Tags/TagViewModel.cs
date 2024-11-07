using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.Api.Tags;

public record struct TagViewModel
{
    /* corresponding client-side model
     * 
        export interface Tag {
          name: string;
          installerDownloadUrl: string;
          installerDownloads: number;
          dateCreated: Date;
          dateUpdated: Date;
          isPreRelease: boolean;

          filename: string;
        }
    */

    public TagViewModel() { }
    public TagViewModel(Tag model)
    {
        Name = model.Name;
        InstallerDownloadUrl = model.InstallerDownloadUrl;
        InstallerDownloads = model.InstallerDownloads;
        DateCreated = model.DateCreated;
        DateTimeUpdated = model.DateTimeUpdated ?? model.DateTimeInserted;
        IsPreRelease = model.IsPreRelease;
    }

    public string Name { get; init; }
    public string InstallerDownloadUrl { get; init; }
    public int InstallerDownloads { get; init; }
    public DateTime DateCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public bool IsPreRelease { get; init; }
}
