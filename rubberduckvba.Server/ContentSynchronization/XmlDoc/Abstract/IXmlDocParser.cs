using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;

public interface IXmlDocParser
{
    string AssetName { get; }
    Task<IEnumerable<FeatureXmlDoc>> ParseAsync(TagGraph tag);
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
