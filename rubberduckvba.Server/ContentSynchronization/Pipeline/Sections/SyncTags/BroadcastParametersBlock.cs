using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class BroadcastParametersBlock : BroadcastBlockBase<SyncRequestParameters, SyncContext>
{
    public BroadcastParametersBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}
