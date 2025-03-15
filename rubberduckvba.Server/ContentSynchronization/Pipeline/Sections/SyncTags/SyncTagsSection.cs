using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class SyncTagsSection : PipelineSection<SyncContext>
{
    public SyncTagsSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger, TagServices tagServices, IGitHubClientService github, IStagingServices staging)
        : base(parent, tokenSource, logger)
    {
        SynchronizeTags = new SynchronizeTagsBlock(this, tokenSource, logger, tagServices, github);
        //ReceiveRequest = new ReceiveRequestBlock(this, tokenSource, logger);
        //BroadcastParameters = new BroadcastParametersBlock(this, tokenSource, logger);
        //AcquireDbMainTag = new AcquireDbMainTagGraphBlock(this, tokenSource, content, logger);
        //AcquireDbNextTag = new AcquireDbNextTagGraphBlock(this, tokenSource, content, logger);
        //JoinDbTags = new DataflowJoinBlock<TagGraph, TagGraph>(this, tokenSource, logger, nameof(JoinDbTags));
        //LoadDbTags = new LoadDbLatestTagsBlock(this, tokenSource, logger);
        //LoadGitHubTags = new LoadGitHubTagsBlock(this, tokenSource, github, logger);
        //JoinTags = new DataflowJoinBlock<SyncContext, SyncContext>(this, tokenSource, logger, nameof(JoinTags));
        //BroadcastTags = new BroadcastTagsBlock(this, tokenSource, logger);
        //StreamGitHubTags = new StreamGitHubTagsBlock(this, tokenSource, logger);
        //GetTagAssets = new GetTagAssetsBlock(this, tokenSource, github, logger);
        //TagBuffer = new TagBufferBlock(this, tokenSource, logger);
        //AccumulateProcessedTags = new AccumulateProcessedTagsBlock(this, tokenSource, logger);
        //SaveTags = new BulkSaveStagingBlock(this, tokenSource, staging, logger);
    }

    //#region blocks
    private SynchronizeTagsBlock SynchronizeTags { get; }
    //private ReceiveRequestBlock ReceiveRequest { get; }
    //private BroadcastParametersBlock BroadcastParameters { get; }
    //private AcquireDbMainTagGraphBlock AcquireDbMainTag { get; }
    //private AcquireDbNextTagGraphBlock AcquireDbNextTag { get; }
    //private DataflowJoinBlock<TagGraph, TagGraph> JoinDbTags { get; }
    //private LoadDbLatestTagsBlock LoadDbTags { get; }
    //private LoadGitHubTagsBlock LoadGitHubTags { get; }
    //private DataflowJoinBlock<SyncContext, SyncContext> JoinTags { get; }
    //private BroadcastTagsBlock BroadcastTags { get; }
    //private StreamGitHubTagsBlock StreamGitHubTags { get; }
    //private GetTagAssetsBlock GetTagAssets { get; }
    //private TagBufferBlock TagBuffer { get; }
    //private AccumulateProcessedTagsBlock AccumulateProcessedTags { get; }
    //private BulkSaveStagingBlock SaveTags { get; }

    public ITargetBlock<TagSyncRequestParameters> InputBlock => SynchronizeTags.Block!;
    public Task OutputTask => SynchronizeTags.Block.Completion;

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        [nameof(SynchronizeTags)] = SynchronizeTags.Block,
        //    [nameof(ReceiveRequest)] = ReceiveRequest.Block,
        //    [nameof(BroadcastParameters)] = BroadcastParameters.Block,
        //    [nameof(AcquireDbMainTag)] = AcquireDbMainTag.Block,
        //    [nameof(AcquireDbNextTag)] = AcquireDbNextTag.Block,
        //    [nameof(JoinDbTags)] = JoinDbTags.Block,
        //    [nameof(LoadDbTags)] = LoadDbTags.Block,
        //    [nameof(LoadGitHubTags)] = LoadGitHubTags.Block,
        //    [nameof(JoinTags)] = JoinTags.Block,
        //    [nameof(BroadcastTags)] = BroadcastTags.Block,
        //    [nameof(StreamGitHubTags)] = StreamGitHubTags.Block,
        //    [nameof(GetTagAssets)] = GetTagAssets.Block,
        //    [nameof(TagBuffer)] = TagBuffer.Block,
        //    [nameof(AccumulateProcessedTags)] = AccumulateProcessedTags.Block,
        //    [nameof(SaveTags)] = SaveTags.Block,
    };
    //#endregion

    public override void CreateBlocks()
    {
        SynchronizeTags.CreateBlock();
        //ReceiveRequest.CreateBlock();
        //BroadcastParameters.CreateBlock(ReceiveRequest);
        //AcquireDbMainTag.CreateBlock(BroadcastParameters);
        //AcquireDbNextTag.CreateBlock(BroadcastParameters);
        //JoinDbTags.CreateBlock(AcquireDbMainTag, AcquireDbNextTag);
        //LoadDbTags.CreateBlock(JoinDbTags);
        //LoadGitHubTags.CreateBlock(LoadDbTags);
        //JoinTags.CreateBlock(LoadDbTags, LoadGitHubTags);
        //BroadcastTags.CreateBlock(JoinTags);
        //StreamGitHubTags.CreateBlock(BroadcastTags);
        //GetTagAssets.CreateBlock(StreamGitHubTags);
        //TagBuffer.CreateBlock(GetTagAssets);
        //AccumulateProcessedTags.CreateBlock(TagBuffer);
        //SaveTags.CreateBlock(AccumulateProcessedTags);
    }
}

public class SynchronizeTagsBlock : ActionBlockBase<TagSyncRequestParameters, SyncContext>
{
    private readonly IGitHubClientService _github;
    private readonly TagServices _tagServices;

    public SynchronizeTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        TagServices tagServices,
        IGitHubClientService github)
        : base(parent, tokenSource, logger)
    {
        _tagServices = tagServices;
        _github = github;
    }

    protected override async Task ActionAsync(TagSyncRequestParameters input)
    {
        var getGithubTags = _github.GetAllTagsAsync();
        var dbMain = _tagServices.GetLatestTag(false);
        var dbNext = _tagServices.GetLatestTag(true);

        var githubTags = await getGithubTags;
        var (gitHubMain, gitHubNext, _) = githubTags.GetLatestTags();

        var mergedMain = (dbMain ?? gitHubMain with { InstallerDownloads = gitHubMain.InstallerDownloads })!;
        var mergedNext = (dbNext ?? gitHubNext with { InstallerDownloads = gitHubNext.InstallerDownloads })!;

        var inserts = new List<TagGraph>();
        var updates = new List<TagGraph>();

        if (dbMain is null)
        {
            inserts.Add(mergedMain);
        }
        else
        {
            updates.Add(mergedMain);
        }

        if (dbNext is null)
        {
            inserts.Add(mergedNext);
        }
        else
        {
            updates.Add(mergedNext);
        }

        if (inserts.Any())
        {
            _tagServices.Create(inserts);
        }
        if (updates.Any())
        {
            _tagServices.Update(updates);
        }
    }
}