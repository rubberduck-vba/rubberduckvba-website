using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class BroadcastTagAssetsBlock : TransformManyBlockBase<TagGraph, TagAsset, SyncContext>
{
    public BroadcastTagAssetsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<TagAsset> Transform(TagGraph input) => input.Assets;
}
