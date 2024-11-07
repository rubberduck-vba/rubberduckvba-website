using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using rubberduckvba.Server;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace rubberduckvba.Server.Services;

public interface IGitHubClientService
{
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    Task<IEnumerable<TagGraph>> GetAllTagsAsync();
    Task<TagGraph> GetTagAsync(string token, string name);
    Task<IEnumerable<InspectionDefaultConfig>> GetCodeAnalysisDefaultsConfigAsync();
}

public class GitHubClientService(IOptions<GitHubSettings> configuration, ILogger<ServiceLogger> logger) : IGitHubClientService
{
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        var config = configuration.Value;
        var credentials = new Credentials(token);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));

        var orgs = await client.Organization.GetAllForCurrent();
        var isOrgMember = orgs.Any(e => e.Id == config.RubberduckOrgId);
        if (!isOrgMember)
        {
            return null;
        }

        var user = await client.User.Current();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, config.OwnerOrg),
            new Claim("access_token", token)
        });
        return new ClaimsPrincipal(identity);
    }

    public async Task<IEnumerable<TagGraph>> GetAllTagsAsync()
    {
        var config = configuration.Value;
        var credentials = new Credentials(config.OrgToken);
        var client = new GitHubClient(new ProductHeaderValue(config.UserAgent), new InMemoryCredentialStore(credentials));

        var releases = await client.Repository.Release.GetAll(config.OwnerOrg, config.Rubberduck, new ApiOptions { PageCount = 1, PageSize = 10 });

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

    public async Task<TagGraph> GetTagAsync(string token, string name)
    {
        var config = configuration.Value;
        var credentials = new Credentials(config.OrgToken);
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
