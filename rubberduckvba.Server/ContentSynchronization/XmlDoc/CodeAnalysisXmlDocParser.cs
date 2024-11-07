using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public interface ICodeAnalysisXmlDocParserFactory
{
    /// <summary>
    /// Creates a new CodeAnalysisXmlDocParser service instance.
    /// </summary>
    /// <param name="inspectionDefaults">Default inspection severity levels, obtained from Rubberduck.WebApi.Services.Abstract.IGitHubDataService</param>
    IXmlDocParser CreateXmlDocParser(IEnumerable<InspectionDefaultConfig> inspectionDefaults);
}

public class CodeAnalysisXmlDocParserFactory : ICodeAnalysisXmlDocParserFactory
{
    private readonly IRubberduckDbService _content;
    private readonly IMarkdownFormattingService _markdown;

    public CodeAnalysisXmlDocParserFactory(IRubberduckDbService content, IMarkdownFormattingService markdown)
    {
        _content = content;
        _markdown = markdown;
    }

    public IXmlDocParser CreateXmlDocParser(IEnumerable<InspectionDefaultConfig> inspectionDefaults)
    {
        return new CodeAnalysisXmlDocParser(_content, _markdown, inspectionDefaults);
    }
}


public class CodeAnalysisXmlDocParser : XmlDocParserBase, ICodeAnalysisXmlDocParser
{
    private readonly IRubberduckDbService _content;
    private readonly IMarkdownFormattingService _markdownService;
    private readonly IDictionary<string, InspectionDefaultConfig> _inspectionDefaults;

    public CodeAnalysisXmlDocParser(IRubberduckDbService content, IMarkdownFormattingService markdownService, IEnumerable<InspectionDefaultConfig> inspectionDefaults)
        : base("Rubberduck.CodeAnalysis.xml")
    {
        _content = content;
        _markdownService = markdownService;
        _inspectionDefaults = inspectionDefaults.ToDictionary(e => e.InspectionName, e => e);
    }

    //protected override async Task<IEnumerable<FeatureXmlDoc>> ParseAsync(int assetId, string tagName, XDocument document, bool isPreRelease)
    //{
    //    var quickfixesFeatureId = await _content.GetFeatureId(RepositoryId.Rubberduck, "QuickFixes")
    //        ?? throw new InvalidOperationException("Could not retrieve a FeatureId for the 'QuickFixes' feature.");
    //    var quickFixes = ReadQuickFixes(assetId, tagName, quickfixesFeatureId, document, !isPreRelease);

    //    var inspectionsFeatureId = await _content.GetFeatureId(RepositoryId.Rubberduck, "Inspections")
    //        ?? throw new InvalidOperationException("Could not retrieve a FeatureId for the 'Inspections' feature.");
    //    var inspections = ReadInspections(assetId, tagName, inspectionsFeatureId, document, !isPreRelease, quickFixes);

    //    return inspections.Concat(quickFixes);
    //}

    private IEnumerable<Inspection> ReadInspections(int assetId, string tagName, int featureId, XDocument doc, bool hasReleased, IEnumerable<QuickFix> quickFixes)
    {
        var nodes = from node in doc.Descendants("member").AsParallel()
                    let name = GetFullTypeNameOrDefault(node, "Inspection")
                    where !string.IsNullOrWhiteSpace(name)
                    let inspectionName = name.Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1)
                    let config = _inspectionDefaults.ContainsKey(inspectionName) ? _inspectionDefaults[inspectionName] : default
                    select (name, node, config);

        var results = new ConcurrentBag<Task<Inspection>>();
        Parallel.ForEach(nodes, info =>
        {
            var xmldoc = new XmlDocInspection(_markdownService);
            results.Add(xmldoc.ParseAsync(assetId, tagName, featureId, quickFixes, info.name, info.node, info.config, !hasReleased));
        });

        return results.Select(t => t.GetAwaiter().GetResult());
    }

    private IEnumerable<QuickFix> ReadQuickFixes(int assetId, string tagName, int featureId, XDocument doc, bool hasReleased)
    {
        var nodes = from node in doc.Descendants("member").AsParallel()
                    let name = GetFullTypeNameOrDefault(node, "QuickFix")
                    where !string.IsNullOrEmpty(name) && node.Descendants(XmlDocSchema.QuickFix.CanFix.ElementName).Any() // this excludes any quickfixes added to main (master) prior to v2.5.0 
                    select (name, node);

        var results = new ConcurrentBag<QuickFix>();
        Parallel.ForEach(nodes, info =>
        {
            var xmldoc = new XmlDocQuickFix();
            results.Add(xmldoc.Parse(info.name, assetId, featureId, info.node, !hasReleased));
        });

        return results;
    }
}
