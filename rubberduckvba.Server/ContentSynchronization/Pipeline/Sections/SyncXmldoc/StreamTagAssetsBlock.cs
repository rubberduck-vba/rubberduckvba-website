using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class StreamTagAssetsBlock : TransformManyBlockBase<TagGraph, TagAsset, SyncContext>
{
    public StreamTagAssetsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<TagAsset> Transform(TagGraph input) => input.Assets;
}
