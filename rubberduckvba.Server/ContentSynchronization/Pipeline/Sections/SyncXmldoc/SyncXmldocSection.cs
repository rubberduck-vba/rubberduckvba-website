using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

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
        XmlDocAnnotationParser xmlAnnotationParser,
        XmlDocQuickFixParser xmlQuickFixParser,
        XmlDocInspectionParser xmlInspectionParser)
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
        StreamLatestTags = new StreamLatestTagsBlock(this, tokenSource, logger);
        StreamTagAssets = new StreamTagAssetsBlock(this, tokenSource, logger);
        BufferXmlAsset = new XmlTagAssetBufferBlock(this, tokenSource, logger);
        DownloadXmlAsset = new DownloadXmlTagAssetBlock(this, tokenSource, logger);
        BroadcastXDocument = new BroadcastXDocumentBlock(this, tokenSource, logger);
        JoinQuickFixes = new DataflowJoinBlock<(TagAsset, XDocument), IEnumerable<QuickFix>>(this, tokenSource, logger, nameof(JoinQuickFixes));
        StreamInspectionNodes = new StreamInspectionNodesBlock(this, tokenSource, logger);
        StreamQuickFixNodes = new StreamQuickFixNodesBlock(this, tokenSource, logger);
        StreamAnnotationNodes = new StreamAnnotationNodesBlock(this, tokenSource, logger);
        ParseXmlDocInspections = new ParseInspectionXElementInfoBlock(this, tokenSource, logger, xmlInspectionParser);
        ParseXmlDocQuickFixes = new ParseQuickFixXElementInfoBlock(this, tokenSource, logger, xmlQuickFixParser);
        ParseXmlDocAnnotations = new ParseAnnotationXElementInfoBlock(this, tokenSource, logger, xmlAnnotationParser);
        MergeInspections = new MergeInspectionsBlock(this, tokenSource, logger, mergeService);
        MergeQuickFixes = new MergeQuickFixesBlock(this, tokenSource, logger, mergeService);
        MergeAnnotations = new MergeAnnotationsBlock(this, tokenSource, logger, mergeService);
        AccumulateProcessedInspections = new AccumulateProcessedInspectionsBlock(this, tokenSource, logger);
        AccumulateProcessedQuickFixes = new AccumulateProcessedQuickFixesBlock(this, tokenSource, logger);
        AccumulateProcessedAnnotations = new AccumulateProcessedAnnotationsBlock(this, tokenSource, logger);
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
    private StreamLatestTagsBlock StreamLatestTags { get; }
    private StreamTagAssetsBlock StreamTagAssets { get; }
    private XmlTagAssetBufferBlock BufferXmlAsset { get; }
    private DownloadXmlTagAssetBlock DownloadXmlAsset { get; }
    private BroadcastXDocumentBlock BroadcastXDocument { get; }

    private StreamQuickFixNodesBlock StreamQuickFixNodes { get; }
    private ParseQuickFixXElementInfoBlock ParseXmlDocQuickFixes { get; }
    private MergeQuickFixesBlock MergeQuickFixes { get; }
    private BroadcastQuickFixesBlock BroadcastQuickFixes { get; }
    private AccumulateProcessedQuickFixesBlock AccumulateProcessedQuickFixes { get; }

    private DataflowJoinBlock<(TagAsset, XDocument), IEnumerable<QuickFix>> JoinQuickFixes { get; }
    private StreamInspectionNodesBlock StreamInspectionNodes { get; }
    private ParseInspectionXElementInfoBlock ParseXmlDocInspections { get; }
    private MergeInspectionsBlock MergeInspections { get; }
    private AccumulateProcessedInspectionsBlock AccumulateProcessedInspections { get; }

    private StreamAnnotationNodesBlock StreamAnnotationNodes { get; }
    private ParseAnnotationXElementInfoBlock ParseXmlDocAnnotations { get; }
    private MergeAnnotationsBlock MergeAnnotations { get; }
    private AccumulateProcessedAnnotationsBlock AccumulateProcessedAnnotations { get; }

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
        [nameof(StreamLatestTags)] = StreamLatestTags.Block,
        [nameof(StreamTagAssets)] = StreamTagAssets.Block,
        [nameof(BufferXmlAsset)] = BufferXmlAsset.Block,
        [nameof(DownloadXmlAsset)] = DownloadXmlAsset.Block,
        [nameof(BroadcastXDocument)] = BroadcastXDocument.Block,
        [nameof(JoinQuickFixes)] = JoinQuickFixes.Block,

        [nameof(StreamQuickFixNodes)] = StreamQuickFixNodes.Block,
        [nameof(ParseXmlDocQuickFixes)] = ParseXmlDocQuickFixes.Block,
        [nameof(MergeQuickFixes)] = MergeQuickFixes.Block,
        [nameof(AccumulateProcessedQuickFixes)] = AccumulateProcessedQuickFixes.Block,

        [nameof(StreamInspectionNodes)] = StreamInspectionNodes.Block,
        [nameof(ParseXmlDocInspections)] = ParseXmlDocInspections.Block,
        [nameof(MergeInspections)] = MergeInspections.Block,
        [nameof(AccumulateProcessedInspections)] = AccumulateProcessedInspections.Block,

        [nameof(StreamAnnotationNodes)] = StreamAnnotationNodes.Block,
        [nameof(ParseXmlDocAnnotations)] = ParseXmlDocAnnotations.Block,
        [nameof(MergeAnnotations)] = MergeAnnotations.Block,
        [nameof(AccumulateProcessedAnnotations)] = AccumulateProcessedAnnotations.Block,

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
        StreamLatestTags.CreateBlock(JoinAsyncSources);
        StreamTagAssets.CreateBlock(StreamLatestTags);
        BufferXmlAsset.CreateBlock(StreamTagAssets);
        DownloadXmlAsset.CreateBlock(BufferXmlAsset);
        BroadcastXDocument.CreateBlock(DownloadXmlAsset);

        StreamQuickFixNodes.CreateBlock(BroadcastXDocument);
        ParseXmlDocQuickFixes.CreateBlock(StreamQuickFixNodes);
        MergeQuickFixes.CreateBlock(() => Context.StagingContext.QuickFixes, AccumulateProcessedQuickFixes.Block.Completion);
        BroadcastQuickFixes.CreateBlock(MergeQuickFixes);
        AccumulateProcessedQuickFixes.CreateBlock(BroadcastQuickFixes);
        JoinQuickFixes.CreateBlock(BroadcastXDocument, BroadcastQuickFixes);

        StreamAnnotationNodes.CreateBlock(BroadcastXDocument);
        ParseXmlDocAnnotations.CreateBlock(StreamAnnotationNodes);
        MergeAnnotations.CreateBlock(() => Context.StagingContext.Annotations, AccumulateProcessedAnnotations.Block.Completion);
        AccumulateProcessedAnnotations.CreateBlock(MergeAnnotations);

        StreamInspectionNodes.CreateBlock(JoinQuickFixes);
        ParseXmlDocInspections.CreateBlock(StreamInspectionNodes);
        MergeInspections.CreateBlock(() => Context.StagingContext.Inspections, AccumulateProcessedInspections.Block.Completion);
        AccumulateProcessedInspections.CreateBlock(MergeInspections);

        SaveStaging.CreateBlock(() => Context.StagingContext,
            AccumulateProcessedInspections.Block.Completion,
            AccumulateProcessedQuickFixes.Block.Completion,
            AccumulateProcessedAnnotations.Block.Completion);
    }
}
