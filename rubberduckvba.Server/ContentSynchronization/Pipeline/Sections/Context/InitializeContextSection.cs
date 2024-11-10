using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;

public class InitializeContextSection : PipelineSection<SyncContext>
{
    public InitializeContextSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger<InitializeContextSection> logger)
        : base(parent, tokenSource, logger)
    {
        ReceiveRequest = new ReceiveRequestBlock(this, tokenSource, logger);
        BroadcastParameters = new BroadcastParametersBlock(this, tokenSource, logger);
    }

    private ReceiveRequestBlock ReceiveRequest { get; }
    private BroadcastParametersBlock BroadcastParameters { get; }

    public ITargetBlock<SyncRequestParameters> InputBlock => ReceiveRequest.Block;
    public Task OutputTask => BroadcastParameters.Block.Completion;

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        [nameof(ReceiveRequest)] = ReceiveRequest.Block,
        [nameof(BroadcastParameters)] = BroadcastParameters.Block,
    };

    public override void CreateBlocks()
    {
        ReceiveRequest.CreateBlock();
        BroadcastParameters.CreateBlock(sources: ReceiveRequest.Block);
    }
}