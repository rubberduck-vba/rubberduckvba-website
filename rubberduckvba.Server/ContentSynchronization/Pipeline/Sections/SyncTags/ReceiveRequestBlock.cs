using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class ReceiveRequestBlock : TransformBlockBase<SyncRequestParameters, SyncRequestParameters, SyncContext>
{
    public ReceiveRequestBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override SyncRequestParameters Transform(SyncRequestParameters input)
    {
        Context.LoadParameters(input);
        return input;
    }
}
