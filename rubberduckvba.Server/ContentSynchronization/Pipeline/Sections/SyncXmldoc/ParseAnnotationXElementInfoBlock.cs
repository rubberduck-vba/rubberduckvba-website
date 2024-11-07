using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseAnnotationXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info), Annotation, SyncContext>
{
    public ParseAnnotationXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override Annotation Transform((TagAsset asset, XElementInfo info) input)
    {
        var feature = Context.Features["Annotations"];
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);

        var parser = new XmlDocAnnotation(input.info.Name, input.info.Element, isPreRelease);
        var result = parser.Parse(input.asset.Id, feature.Id);

        return result ?? throw new InvalidOperationException("Failed to parse annotation xmldoc");
    }
}
