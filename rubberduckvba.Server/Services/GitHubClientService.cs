using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using rubberduckvba.Server;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace rubberduckvba.Server.Services;

public interface IGitHubClientService
{
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    Task<IEnumerable<TagGraph>> GetAllTagsAsync();
    Task<TagGraph> GetTagAsync(string? token, string name);
    Task<IEnumerable<InspectionDefaultConfig>> GetCodeAnalysisDefaultsConfigAsync();
}

public class GitHubClientService(IOptions<GitHubSettings> configuration, ILogger<ServiceLogger> logger) : IGitHubClientService
{
    private class ReleaseComparer : IEqualityComparer<Release>
    {
        public bool Equals(Release? x, Release? y) => x?.Name == y?.Name;

        public int GetHashCode([DisallowNull] Release obj) => HashCode.Combine(obj.Name);
    }

    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string? token)
    {
        if (token is null)
        {
            return null;
        }

        var config = configuration.Value;
        var credentials = new Credentials(token);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));

        var (name, role) = await DetermineUserRole(client);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Authentication, token),
            new("access_token", token)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "github"));
    }

    private static async Task<(string name, string role)> DetermineUserRole(GitHubClient client)
    {
        var role = RDConstants.Roles.ReaderRole;

        var user = await client.User.Current();
        if (!user.Suspended)
        {
            // only authenticated GitHub users in good standing can submit edits
            role = RDConstants.Roles.WriterRole;

            var orgs = await client.Organization.GetAllForCurrent();
            if (orgs.SingleOrDefault(e => e.Id == RDConstants.Org.OrganisationId) is not null)
            {
                var teams = await client.Organization.Team.GetAllForCurrent();

                // members of the Rubberduck organization are welcome to review/approve/reject suggested changes
                // NOTE: opportunity for eventual distinction between members and contributors
                role = RDConstants.Roles.ReviewerRole;

                //// members of the contributors team can review/approve/reject suggested changes
                //if (teams.SingleOrDefault(e => e.Name == RDConstants.Org.ContributorsTeam) is not null)
                //{
                //    role = RDConstants.Roles.ReviewerRole;
                //}

                // authenticated org members in the WebAdmin team can manage the site and approve their own changes
                if (teams.SingleOrDefault(e => e.Name == RDConstants.Org.WebAdminTeam) is not null)
                {
                    role = RDConstants.Roles.AdminRole;
                }
            }
        }

        return (user.Login, role);
    }

    public async Task<IEnumerable<TagGraph>> GetAllTagsAsync()
    {
        var config = configuration.Value;
        var credentials = new Credentials(config.OrgToken);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));


        var getReleases = client.Repository.Release.GetAll(config.OwnerOrg, config.Rubberduck, new ApiOptions { PageCount = 1, PageSize = 10 });
        var getLatest = client.Repository.Release.GetLatest(config.OwnerOrg, config.Rubberduck);
        await Task.WhenAll(getReleases, getLatest);

        var releases = (await getReleases).Append(await getLatest).ToHashSet(new ReleaseComparer());

        return (from release in releases
                let installer = release.Assets.SingleOrDefault(asset => asset.Name.EndsWith(".exe") && asset.Name.StartsWith("Rubberduck.Setup"))
                select new TagGraph
                {
                    ReleaseId = release.Id,
                    Name = release.TagName,
                    DateCreated = release.CreatedAt.Date,
                    IsPreRelease = release.Prerelease,
                    InstallerDownloads = installer?.DownloadCount ?? 0,
                    InstallerDownloadUrl = installer?.BrowserDownloadUrl ?? string.Empty,
                    Assets = (from asset in release.Assets
                              where asset.Name.EndsWith(".xml")
                              select new TagAsset
                              {
                                  Name = asset.Name,
                                  DownloadUrl = asset.BrowserDownloadUrl
                              }).ToImmutableArray()
                }).ToImmutableArray();
    }

    public async Task<TagGraph> GetTagAsync(string? token, string name)
    {
        var config = configuration.Value;
        var credentials = new Credentials(token ?? config.OrgToken);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));

        var release = await client.Repository.Release.Get(config.OwnerOrg, config.Rubberduck, name);
        var installer = release.Assets.SingleOrDefault(asset => asset.Name.EndsWith(".exe") && asset.Name.StartsWith("Rubberduck.Setup"));

        return new TagGraph
        {
            ReleaseId = release.Id,
            Name = release.TagName,
            DateCreated = release.CreatedAt.Date,
            IsPreRelease = release.Prerelease,
            InstallerDownloads = installer?.DownloadCount ?? 0,
            InstallerDownloadUrl = installer?.BrowserDownloadUrl ?? string.Empty,
            Assets = (from asset in release.Assets
                      where asset.Name.EndsWith(".xml")
                      select new TagAsset
                      {
                          Name = asset.Name,
                          DownloadUrl = asset.BrowserDownloadUrl
                      }).ToImmutableArray()
        };
    }

    public async Task<IEnumerable<InspectionDefaultConfig>> GetCodeAnalysisDefaultsConfigAsync()
    {
        var config = configuration.Value;
        var credentials = new Credentials(config.OrgToken);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));
        var path = config.CodeInspectionDefaultSettingsUri;

        var bytes = await client.Repository.Content.GetRawContent(config.OwnerOrg, config.Rubberduck, path);
        logger.LogTrace(nameof(GetCodeAnalysisDefaultsConfigAsync) + " | GetRawContent returned {bytes} bytes", bytes.Length);

        var document = ParseCodeInspectionSettings(bytes);
        logger.LogTrace(nameof(GetCodeAnalysisDefaultsConfigAsync) + " | ParseCodeInspectionSettings returned XDocument from supplied bytes");

        var results = from e in document.Descendants(XmlDocSchema.Inspection.Config.ElementName)
                      let name = e.Attribute(XmlDocSchema.Inspection.Config.NameAttribute)?.Value
                      let type = e.Attribute(XmlDocSchema.Inspection.Config.InspectionTypeAttribute)?.Value
                      let severity = e.Attribute(XmlDocSchema.Inspection.Config.SeverityAttribute)?.Value
                      where name != null && type != null && severity != null
                      select new InspectionDefaultConfig
                      {
                          InspectionName = name,
                          InspectionType = type,
                          DefaultSeverity = severity,
                      };

        return results.ToList();
    }

    private XDocument ParseCodeInspectionSettings(byte[] rawContent)
    {
        /*
<?xml version='1.0' encoding='utf-8'?>
<SettingsFile xmlns="http://schemas.microsoft.com/VisualStudio/2004/01/settings" CurrentProfile="(Default)" GeneratedClassNamespace="Rubberduck.CodeAnalysis.Properties" GeneratedClassName="CodeInspectionDefaults">
  <Profiles />
  <Settings>
    <Setting Name="CodeInspectionSettings" Type="Rubberduck.CodeAnalysis.Settings.CodeInspectionSettings" Scope="Application">
      <Value Profile="(Default)">&lt;?xml version="1.0" encoding="utf-16"?&gt;
&lt;CodeInspectionSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"&gt;
  &lt;CodeInspections&gt;
    &lt;CodeInspection Name="BooleanAssignedInIfElseInspection" Severity="Warning" InspectionType="NamingAndConventionsIssues" /&gt;
    &lt;CodeInspection Name="ObsoleteErrorSyntaxInspection" Severity="Suggestion" InspectionType="LanguageOpportunities" /&gt;
    &lt;CodeInspection Name="StopKeywordInspection" Severity="Suggestion" InspectionType="CodeQualityIssues" /&gt;
    &lt;CodeInspection Name="UnhandledOnErrorResumeNextInspection" Severity="Warning" InspectionType="CodeQualityIssues" /&gt;
    ...
  &lt;/CodeInspections&gt;
  &lt;WhitelistedIdentifiers /&gt;
  &lt;RunInspectionsOnSuccessfulParse&gt;true&lt;/RunInspectionsOnSuccessfulParse&gt;
&lt;/CodeInspectionSettings&gt;</Value>
    </Setting>
  </Settings>
</SettingsFile>
        */
        var encoded = Encoding.UTF8.GetString(rawContent);

        const string encodedTagOpen = "&lt;CodeInspections&gt;";
        var startIndex = encoded.IndexOf(encodedTagOpen);

        const string encodedTagClose = "&lt;/CodeInspections&gt;";
        var endIndex = encoded.IndexOf(encodedTagClose);

        var length = endIndex - startIndex + encodedTagClose.Length;
        var encodedDocument = encoded.Substring(startIndex, length);
        var decoded = HttpUtility.HtmlDecode(encodedDocument);

        var document = XDocument.Parse(decoded);
        return document;
    }
}
