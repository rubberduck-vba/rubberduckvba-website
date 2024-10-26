using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class BroadcastTagsBlock : BroadcastBlockBase<Tuple<SyncContext, SyncContext>, SyncContext>
{
    public BroadcastTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}
