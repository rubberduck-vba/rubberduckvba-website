using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseAnnotationXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info), Annotation, SyncContext>
{
    private readonly XmlDocAnnotationParser _parser;

    public ParseAnnotationXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, XmlDocAnnotationParser parser)
        : base(parent, tokenSource, logger)
    {
        _parser = parser;
    }

    public override Annotation Transform((TagAsset asset, XElementInfo info) input)
    {
        var feature = Context.Features["Annotations"];
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);

        var result = _parser.Parse(input.asset.Id, feature.Id, input.info.Name, input.info.Element, isPreRelease);

        return result ?? throw new InvalidOperationException("Failed to parse annotation xmldoc");
    }
}
