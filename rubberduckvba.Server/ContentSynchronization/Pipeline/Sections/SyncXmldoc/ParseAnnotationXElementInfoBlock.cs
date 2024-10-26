using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseAnnotationXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info), FeatureXmlDoc, SyncContext>
{
    public ParseAnnotationXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override FeatureXmlDoc Transform((TagAsset asset, XElementInfo info) input)
    {
        if (input.asset is null)
        {
            return null;
        }

        var feature = Context.Features["Annotations"];
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);

        var parser = new XmlDocAnnotation(input.info.Name, input.info.Element, isPreRelease);
        var result = parser.Parse(input.asset.Id, feature.Id);

        return result;
    }
}
