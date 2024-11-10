using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;

public abstract class XmlDocParserBase : IXmlDocParser
{
    protected XmlDocParserBase(string assetName)
    {
        AssetName = assetName;
    }

    public string AssetName { get; }

    public async Task<XDocument> GetTagAssetXDocumentAsync(TagGraph tag)
    {
        if (tag is null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        var asset = tag.Assets.SingleOrDefault(a => a.Name.Contains(AssetName))
            ?? throw new InvalidOperationException($"Asset '{AssetName}' was not found under tag {tag.Name}.");

        if (Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var uri) && uri.Host != "github.com")
        {
            throw new UriFormatException($"Unexpected host in download URL '{uri}' from asset ID {asset.Id} (tag ID {tag.Id}, '{tag.Name}').");
        }

        using var client = new HttpClient();
        using var response = await client.GetAsync(uri);

        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();

        return XDocument.Load(stream, LoadOptions.None);
    }

    protected static string? GetFullTypeNameOrDefault(XElement memberNode, string suffix)
    {
        var name = memberNode.Attribute("name")?.Value;
        if (name is null || !name.StartsWith("T:") || !name.EndsWith(suffix))
        {
            return default;
        }

        return name[2..];
    }

    public virtual IEnumerable<Inspection> ParseInspections(XDocument document) => [];

    public virtual IEnumerable<QuickFix> ParseQuickFixes(XDocument document) => [];

    public virtual IEnumerable<Annotation> ParseAnnotations(XDocument document) => [];
}
