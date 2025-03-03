using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class SyncTagsSection : PipelineSection<SyncContext>
{
    public SyncTagsSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger, IRubberduckDbService content, IGitHubClientService github, IStagingServices staging)
        : base(parent, tokenSource, logger)
    {
        ReceiveRequest = new ReceiveRequestBlock(this, tokenSource, logger);
        BroadcastParameters = new BroadcastParametersBlock(this, tokenSource, logger);
        AcquireDbMainTag = new AcquireDbMainTagGraphBlock(this, tokenSource, content, logger);
        AcquireDbNextTag = new AcquireDbNextTagGraphBlock(this, tokenSource, content, logger);
        JoinDbTags = new DataflowJoinBlock<TagGraph, TagGraph>(this, tokenSource, logger, nameof(JoinDbTags));
        LoadDbTags = new LoadDbLatestTagsBlock(this, tokenSource, logger);
        LoadGitHubTags = new LoadGitHubTagsBlock(this, tokenSource, github, logger);
        JoinTags = new DataflowJoinBlock<SyncContext, SyncContext>(this, tokenSource, logger, nameof(JoinTags));
        BroadcastTags = new BroadcastTagsBlock(this, tokenSource, logger);
        StreamGitHubTags = new StreamGitHubTagsBlock(this, tokenSource, logger);
        GetTagAssets = new GetTagAssetsBlock(this, tokenSource, github, logger);
        TagBuffer = new TagBufferBlock(this, tokenSource, logger);
        AccumulateProcessedTags = new AccumulateProcessedTagsBlock(this, tokenSource, logger);
        SaveTags = new BulkSaveStagingBlock(this, tokenSource, staging, logger);
    }

    #region blocks
    private ReceiveRequestBlock ReceiveRequest { get; }
    private BroadcastParametersBlock BroadcastParameters { get; }
    private AcquireDbMainTagGraphBlock AcquireDbMainTag { get; }
    private AcquireDbNextTagGraphBlock AcquireDbNextTag { get; }
    private DataflowJoinBlock<TagGraph, TagGraph> JoinDbTags { get; }
    private LoadDbLatestTagsBlock LoadDbTags { get; }
    private LoadGitHubTagsBlock LoadGitHubTags { get; }
    private DataflowJoinBlock<SyncContext, SyncContext> JoinTags { get; }
    private BroadcastTagsBlock BroadcastTags { get; }
    private StreamGitHubTagsBlock StreamGitHubTags { get; }
    private GetTagAssetsBlock GetTagAssets { get; }
    private TagBufferBlock TagBuffer { get; }
    private AccumulateProcessedTagsBlock AccumulateProcessedTags { get; }
    private BulkSaveStagingBlock SaveTags { get; }

    public ITargetBlock<SyncRequestParameters> InputBlock => ReceiveRequest.Block;
    public Task OutputTask => SaveTags.Block.Completion;

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        [nameof(ReceiveRequest)] = ReceiveRequest.Block,
        [nameof(BroadcastParameters)] = BroadcastParameters.Block,
        [nameof(AcquireDbMainTag)] = AcquireDbMainTag.Block,
        [nameof(AcquireDbNextTag)] = AcquireDbNextTag.Block,
        [nameof(JoinDbTags)] = JoinDbTags.Block,
        [nameof(LoadDbTags)] = LoadDbTags.Block,
        [nameof(LoadGitHubTags)] = LoadGitHubTags.Block,
        [nameof(JoinTags)] = JoinTags.Block,
        [nameof(BroadcastTags)] = BroadcastTags.Block,
        [nameof(StreamGitHubTags)] = StreamGitHubTags.Block,
        [nameof(GetTagAssets)] = GetTagAssets.Block,
        [nameof(TagBuffer)] = TagBuffer.Block,
        [nameof(AccumulateProcessedTags)] = AccumulateProcessedTags.Block,
        [nameof(SaveTags)] = SaveTags.Block,
    };
    #endregion

    public override void CreateBlocks()
    {
        ReceiveRequest.CreateBlock();
        BroadcastParameters.CreateBlock(ReceiveRequest);
        AcquireDbMainTag.CreateBlock(BroadcastParameters);
        AcquireDbNextTag.CreateBlock(BroadcastParameters);
        JoinDbTags.CreateBlock(AcquireDbMainTag, AcquireDbNextTag);
        LoadDbTags.CreateBlock(JoinDbTags);
        LoadGitHubTags.CreateBlock(LoadDbTags);
        JoinTags.CreateBlock(LoadDbTags, LoadGitHubTags);
        BroadcastTags.CreateBlock(JoinTags);
        StreamGitHubTags.CreateBlock(BroadcastTags);
        GetTagAssets.CreateBlock(StreamGitHubTags);
        TagBuffer.CreateBlock(GetTagAssets);
        AccumulateProcessedTags.CreateBlock(TagBuffer);
        SaveTags.CreateBlock(AccumulateProcessedTags);
    }
}
