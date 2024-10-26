using System.Xml.Linq;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;

public abstract class XmlDocParserBase : IXmlDocParser
{
    protected XmlDocParserBase(string assetName)
    {
        AssetName = assetName;
    }

    public string AssetName { get; }

    public async Task<IEnumerable<FeatureXmlDoc>> ParseAsync(TagGraph tag)
    {
        if (tag is null)
        {
            return Enumerable.Empty<FeatureXmlDoc>();
        }

        var asset = tag.Assets.SingleOrDefault(a => a.Name.Contains(AssetName))
            ?? throw new InvalidOperationException($"Asset '{AssetName}' was not found under tag {tag.Name}.");

        if (Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var uri) && uri.Host != "github.com")
        {
            throw new UriFormatException($"Unexpected host in download URL '{uri}' from asset ID {asset.Id} (tag ID {tag.Id}, '{tag.Name}').");
        }

        using (var client = new HttpClient())
        using (var response = await client.GetAsync(uri))
        {
            if (response.IsSuccessStatusCode)
            {
                XDocument document;
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    document = XDocument.Load(stream, LoadOptions.None);
                }
                var items = await ParseAsync(asset.Id, tag.Name, document, tag.IsPreRelease);
                return items;
            }
        }

        return Enumerable.Empty<FeatureXmlDoc>();
    }

    protected abstract Task<IEnumerable<FeatureXmlDoc>> ParseAsync(int assetId, string tagName, XDocument document, bool isPreRelease);

    protected static string GetFullTypeNameOrDefault(XElement memberNode, string suffix)
    {
        var name = memberNode.Attribute("name")?.Value;
        if (name == null || !name.StartsWith("T:") || !name.EndsWith(suffix))
        {
            return default;
        }

        return name.Substring(2);
    }
}
