using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

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
