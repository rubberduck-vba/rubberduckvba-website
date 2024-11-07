using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class SyncXmldocSection : PipelineSection<SyncContext>
{
    public SyncXmldocSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger,
        IRubberduckDbService content,
        IRepository<InspectionEntity> inspections,
        IRepository<QuickFixEntity> quickfixes,
        IRepository<AnnotationEntity> annotations,
        IGitHubClientService github,
        IXmlDocMerge mergeService,
        IStagingServices staging,
        IMarkdownFormattingService markdownService)
        : base(parent, tokenSource, logger)
    {
        ReceiveRequest = new ReceiveRequestBlock(this, tokenSource, logger);
        BroadcastParameters = new BroadcastParametersBlock(this, tokenSource, logger);
        LoadInspectionDefaultConfig = new LoadInspectionDefaultConfigBlock(this, tokenSource, github, logger);
        LoadFeatures = new LoadFeaturesBlock(this, tokenSource, content, logger);
        LoadDbFeatureItems = new LoadDbFeatureItemsBlock(this, tokenSource, logger, inspections, quickfixes, annotations);
        AcquireDbMainTagGraph = new AcquireDbMainTagGraphBlock(this, tokenSource, content, logger);
        AcquireDbNextTagGraph = new AcquireDbNextTagGraphBlock(this, tokenSource, content, logger);
        AcquireDbTags = new AcquireDbTagsBlock(this, tokenSource, logger, content);
        JoinDbTags = new DataflowJoinBlock<TagGraph, TagGraph, IEnumerable<Tag>>(this, tokenSource, logger, nameof(JoinDbTags));
        LoadDbTags = new LoadDbTagsBlock(this, tokenSource, logger);
        JoinAsyncSources = new DataflowJoinBlock<SyncContext, SyncContext, SyncContext>(this, tokenSource, logger, nameof(JoinAsyncSources));
        BroadcastTags = new BroadcastLatestTagsBlock(this, tokenSource, logger);
        BroadcastAssets = new BroadcastTagAssetsBlock(this, tokenSource, logger);
        StreamXmlAssets = new StreamXmlAssetsBlock(this, tokenSource, logger);
        DownloadXmlAsset = new DownloadXmlAssetBlock(this, tokenSource, logger);
        BroadcastXDocument = new BroadcastXDocumentBlock(this, tokenSource, logger);
        GetInspectionNodes = new GetInspectionNodesBlock(this, tokenSource, logger);
        GetQuickFixNodes = new GetQuickFixNodesBlock(this, tokenSource, logger);
        GetAnnotationNodes = new GetAnnotationNodesBlock(this, tokenSource, logger);
        ParseInspectionInfo = new ParseInspectionXElementInfoBlock(this, tokenSource, logger, markdownService);
        ParseQuickFixInfo = new ParseQuickFixXElementInfoBlock(this, tokenSource, logger);
        ParseAnnotationInfo = new ParseAnnotationXElementInfoBlock(this, tokenSource, logger);
        FeatureItemBuffer = new FeatureItemBufferBlock(this, tokenSource, logger);
        MergeInspections = new MergeInspectionsBlock(this, tokenSource, logger, mergeService);
        MergeQuickFixes = new MergeQuickFixesBlock(this, tokenSource, logger, mergeService);
        MergeAnnotations = new MergeAnnotationsBlock(this, tokenSource, logger, mergeService);
        AccumulateProcessedItems = new AccumulateProcessedFeatureItemsBlock(this, tokenSource, logger);
        SaveStaging = new BulkSaveStagingBlock(this, tokenSource, staging, logger);
    }

    #region blocks
    private ReceiveRequestBlock ReceiveRequest { get; }
    private BroadcastParametersBlock BroadcastParameters { get; }
    private LoadInspectionDefaultConfigBlock LoadInspectionDefaultConfig { get; }
    private LoadFeaturesBlock LoadFeatures { get; }
    private LoadDbFeatureItemsBlock LoadDbFeatureItems { get; }
    private AcquireDbMainTagGraphBlock AcquireDbMainTagGraph { get; }
    private AcquireDbNextTagGraphBlock AcquireDbNextTagGraph { get; }
    private AcquireDbTagsBlock AcquireDbTags { get; }
    private DataflowJoinBlock<TagGraph, TagGraph, IEnumerable<Tag>> JoinDbTags { get; }
    private LoadDbTagsBlock LoadDbTags { get; }
    private DataflowJoinBlock<SyncContext, SyncContext, SyncContext> JoinAsyncSources { get; }
    private BroadcastLatestTagsBlock BroadcastTags { get; }
    private BroadcastTagAssetsBlock BroadcastAssets { get; }
    private StreamXmlAssetsBlock StreamXmlAssets { get; }
    private DownloadXmlAssetBlock DownloadXmlAsset { get; }
    private BroadcastXDocumentBlock BroadcastXDocument { get; }
    private GetInspectionNodesBlock GetInspectionNodes { get; }
    private GetQuickFixNodesBlock GetQuickFixNodes { get; }
    private GetAnnotationNodesBlock GetAnnotationNodes { get; }
    private ParseInspectionXElementInfoBlock ParseInspectionInfo { get; }
    private ParseQuickFixXElementInfoBlock ParseQuickFixInfo { get; }
    private ParseAnnotationXElementInfoBlock ParseAnnotationInfo { get; }
    private FeatureItemBufferBlock FeatureItemBuffer { get; }
    private MergeInspectionsBlock MergeInspections { get; }
    private MergeQuickFixesBlock MergeQuickFixes { get; }
    private MergeAnnotationsBlock MergeAnnotations { get; }
    private AccumulateProcessedFeatureItemsBlock AccumulateProcessedItems { get; }
    private BulkSaveStagingBlock SaveStaging { get; }

    public ITargetBlock<XmldocSyncRequestParameters> InputBlock => ReceiveRequest.Block;
    public IDataflowBlock OutputBlock => SaveStaging.Block;

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        [nameof(ReceiveRequest)] = ReceiveRequest.Block,
        [nameof(BroadcastParameters)] = BroadcastParameters.Block,
        [nameof(LoadInspectionDefaultConfig)] = LoadInspectionDefaultConfig.Block,
        [nameof(LoadFeatures)] = LoadFeatures.Block,
        [nameof(LoadDbFeatureItems)] = LoadDbFeatureItems.Block,
        [nameof(AcquireDbTags)] = AcquireDbTags.Block,
        [nameof(AcquireDbMainTagGraph)] = AcquireDbMainTagGraph.Block,
        [nameof(AcquireDbNextTagGraph)] = AcquireDbNextTagGraph.Block,
        [nameof(JoinDbTags)] = JoinDbTags.Block,
        [nameof(LoadDbTags)] = LoadDbTags.Block,
        [nameof(JoinAsyncSources)] = JoinAsyncSources.Block,
        [nameof(BroadcastTags)] = BroadcastTags.Block,
        [nameof(BroadcastAssets)] = BroadcastAssets.Block,
        [nameof(StreamXmlAssets)] = StreamXmlAssets.Block,
        [nameof(DownloadXmlAsset)] = DownloadXmlAsset.Block,
        [nameof(BroadcastXDocument)] = BroadcastXDocument.Block,
        [nameof(GetInspectionNodes)] = GetInspectionNodes.Block,
        [nameof(GetQuickFixNodes)] = GetQuickFixNodes.Block,
        [nameof(GetAnnotationNodes)] = GetAnnotationNodes.Block,
        [nameof(ParseInspectionInfo)] = ParseInspectionInfo.Block,
        [nameof(ParseQuickFixInfo)] = ParseQuickFixInfo.Block,
        [nameof(ParseAnnotationInfo)] = ParseAnnotationInfo.Block,
        [nameof(FeatureItemBuffer)] = FeatureItemBuffer.Block,
        [nameof(MergeInspections)] = MergeInspections.Block,
        [nameof(MergeQuickFixes)] = MergeQuickFixes.Block,
        [nameof(MergeAnnotations)] = MergeAnnotations.Block,
        [nameof(AccumulateProcessedItems)] = AccumulateProcessedItems.Block,
        [nameof(SaveStaging)] = SaveStaging.Block,
    };
    #endregion

    public override void CreateBlocks()
    {
        ReceiveRequest.CreateBlock();
        BroadcastParameters.CreateBlock(ReceiveRequest);
        LoadInspectionDefaultConfig.CreateBlock(BroadcastParameters);
        LoadFeatures.CreateBlock(BroadcastParameters);
        LoadDbFeatureItems.CreateBlock(BroadcastParameters);
        AcquireDbMainTagGraph.CreateBlock(BroadcastParameters);
        AcquireDbNextTagGraph.CreateBlock(BroadcastParameters);
        AcquireDbTags.CreateBlock(BroadcastParameters);
        JoinDbTags.CreateBlock(AcquireDbMainTagGraph, AcquireDbNextTagGraph, AcquireDbTags);
        LoadDbTags.CreateBlock(JoinDbTags);
        JoinAsyncSources.CreateBlock(LoadDbTags, LoadInspectionDefaultConfig, LoadFeatures);
        BroadcastTags.CreateBlock(JoinAsyncSources);
        BroadcastAssets.CreateBlock(BroadcastTags);
        StreamXmlAssets.CreateBlock(BroadcastAssets);
        DownloadXmlAsset.CreateBlock(StreamXmlAssets);
        BroadcastXDocument.CreateBlock(DownloadXmlAsset);
        GetInspectionNodes.CreateBlock(BroadcastXDocument);
        GetQuickFixNodes.CreateBlock(BroadcastXDocument);
        GetAnnotationNodes.CreateBlock(BroadcastXDocument);
        ParseInspectionInfo.CreateBlock(GetInspectionNodes);
        ParseQuickFixInfo.CreateBlock(GetQuickFixNodes);
        ParseAnnotationInfo.CreateBlock(GetAnnotationNodes);
        FeatureItemBuffer.CreateBlock(ParseInspectionInfo, ParseQuickFixInfo, ParseAnnotationInfo);
        AccumulateProcessedItems.CreateBlock(FeatureItemBuffer);
        MergeInspections.CreateBlock(() => Context.StagingContext.NewInspections, AccumulateProcessedItems.Block.Completion);
        SaveStaging.CreateBlock(MergeInspections);
    }
}
