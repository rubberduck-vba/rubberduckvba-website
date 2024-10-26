using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class BroadcastParametersBlock : BroadcastBlockBase<SyncRequestParameters, SyncContext>
{
    public BroadcastParametersBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}
