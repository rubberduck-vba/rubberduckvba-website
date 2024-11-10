using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;

public interface IXmlDocParser
{
    string AssetName { get; }
    IEnumerable<Inspection> ParseInspections(XDocument document);
    IEnumerable<QuickFix> ParseQuickFixes(XDocument document);
    IEnumerable<Annotation> ParseAnnotations(XDocument document);
}

/// <summary>
/// Downloads and processes Rubberduck.CodeAnalysis xmldoc asset for a tag.
/// </summary>
/// <remarks>DO NOT INJECT - use ICodeAnalysisXmlDocParserFactory instead.</remarks>
public interface ICodeAnalysisXmlDocParser : IXmlDocParser { /* DI/IoC marker interface */ }

/// <summary>
/// Downloads and processes Rubberduck.Parsing xmldoc asset for a tag.
/// </summary>
public interface IParsingXmlDocParser : IXmlDocParser { /* DI/IoC marker interface */ }
