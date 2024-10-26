using System.Xml.Linq;
using System.Collections.Concurrent;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc;

public class ParsingXmlDocParser : XmlDocParserBase, IParsingXmlDocParser
{
    private readonly IRubberduckDbService _content;

    public ParsingXmlDocParser(IRubberduckDbService content)
        : base("Rubberduck.Parsing.xml")
    {
        _content = content;
    }

    protected override async Task<IEnumerable<FeatureXmlDoc>> ParseAsync(int assetId, string tagName, XDocument document, bool isPreRelease)
    {
        var featureId = await _content.GetFeatureId(RepositoryId.Rubberduck, "Annotations")
            ?? throw new InvalidOperationException("Could not retrieve a FeatureId for the 'Annotations' feature.");
        return await Task.FromResult(ReadAnnotations(assetId, tagName, featureId, document, !isPreRelease));
    }

    private IEnumerable<FeatureXmlDoc> ReadAnnotations(int assetId, string tagName, int featureId, XDocument doc, bool hasReleased)
    {
        var nodes = from node in doc.Descendants("member").AsParallel()
                    let name = GetFullTypeNameOrDefault(node, "Annotation")
                    where !string.IsNullOrWhiteSpace(name)
                    select (name, node);

        var results = new ConcurrentBag<FeatureXmlDoc>();
        Parallel.ForEach(nodes, info =>
        {
            var xmldoc = new XmlDocAnnotation(info.name, info.node, !hasReleased);
            var item = xmldoc.Parse(assetId, featureId);
            results.Add(item);
        });

        return results;
    }
}
