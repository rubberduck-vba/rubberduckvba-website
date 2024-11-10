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
        BufferInspections = new InspectionBufferBlock(this, tokenSource, logger);
        BufferQuickFixes = new QuickFixBufferBlock(this, tokenSource, logger);
        BufferAnnotations = new AnnotationBufferBlock(this, tokenSource, logger);
        ParseXmlDocInspections = new ParseInspectionXElementInfoBlock(this, tokenSource, logger, xmlInspectionParser);
        ParseXmlDocQuickFixes = new ParseQuickFixXElementInfoBlock(this, tokenSource, logger, xmlQuickFixParser);
        ParseXmlDocAnnotations = new ParseAnnotationXElementInfoBlock(this, tokenSource, logger, xmlAnnotationParser);
        MergeInspections = new MergeInspectionsBlock(this, tokenSource, logger, mergeService);
        MergeQuickFixes = new MergeQuickFixesBlock(this, tokenSource, logger, mergeService);
        BroadcastQuickFixes = new BroadcastQuickFixesBlock(this, tokenSource, logger);
        BroadcastAnnotations = new BroadcastAnnotationsBlock(this, tokenSource, logger);
        BroadcastInspections = new BroadcastInspectionsBlock(this, tokenSource, logger);
        MergeAnnotations = new MergeAnnotationsBlock(this, tokenSource, logger, mergeService);
        AccumulateProcessedInspections = new AccumulateProcessedInspectionsBlock(this, tokenSource, logger);
        AccumulateProcessedQuickFixes = new AccumulateProcessedQuickFixesBlock(this, tokenSource, logger);
        AccumulateProcessedAnnotations = new AccumulateProcessedAnnotationsBlock(this, tokenSource, logger);
        JoinStagingSources = new DataflowJoinBlock<IEnumerable<Annotation>, IEnumerable<QuickFix>, IEnumerable<Inspection>>(this, tokenSource, logger, nameof(JoinStagingSources));
        PrepareStaging = new PrepareStagingBlock(this, tokenSource, logger);
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
    private QuickFixBufferBlock BufferQuickFixes { get; }
    private ParseQuickFixXElementInfoBlock ParseXmlDocQuickFixes { get; }
    private MergeQuickFixesBlock MergeQuickFixes { get; }
    private BroadcastQuickFixesBlock BroadcastQuickFixes { get; }
    private AccumulateProcessedQuickFixesBlock AccumulateProcessedQuickFixes { get; }

    private DataflowJoinBlock<(TagAsset, XDocument), IEnumerable<QuickFix>> JoinQuickFixes { get; }
    private StreamInspectionNodesBlock StreamInspectionNodes { get; }
    private InspectionBufferBlock BufferInspections { get; }
    private ParseInspectionXElementInfoBlock ParseXmlDocInspections { get; }
    private MergeInspectionsBlock MergeInspections { get; }
    private BroadcastInspectionsBlock BroadcastInspections { get; }
    private AccumulateProcessedInspectionsBlock AccumulateProcessedInspections { get; }

    private StreamAnnotationNodesBlock StreamAnnotationNodes { get; }
    private AnnotationBufferBlock BufferAnnotations { get; }
    private ParseAnnotationXElementInfoBlock ParseXmlDocAnnotations { get; }
    private MergeAnnotationsBlock MergeAnnotations { get; }
    private BroadcastAnnotationsBlock BroadcastAnnotations { get; }
    private AccumulateProcessedAnnotationsBlock AccumulateProcessedAnnotations { get; }

    private DataflowJoinBlock<IEnumerable<Annotation>, IEnumerable<QuickFix>, IEnumerable<Inspection>> JoinStagingSources { get; }
    private PrepareStagingBlock PrepareStaging { get; }
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

        [nameof(StreamAnnotationNodes)] = StreamAnnotationNodes.Block,
        [nameof(ParseXmlDocAnnotations)] = ParseXmlDocAnnotations.Block,
        [nameof(AccumulateProcessedAnnotations)] = AccumulateProcessedAnnotations.Block,
        [nameof(MergeAnnotations)] = MergeAnnotations.Block,
        [nameof(BufferAnnotations)] = BufferAnnotations.Block,
        [nameof(BroadcastAnnotations)] = BroadcastAnnotations.Block,

        [nameof(StreamQuickFixNodes)] = StreamQuickFixNodes.Block,
        [nameof(ParseXmlDocQuickFixes)] = ParseXmlDocQuickFixes.Block,
        [nameof(AccumulateProcessedQuickFixes)] = AccumulateProcessedQuickFixes.Block,
        [nameof(MergeQuickFixes)] = MergeQuickFixes.Block,
        [nameof(BufferQuickFixes)] = BufferQuickFixes.Block,
        [nameof(BroadcastQuickFixes)] = BroadcastQuickFixes.Block,

        [nameof(JoinQuickFixes)] = JoinQuickFixes.Block,
        [nameof(StreamInspectionNodes)] = StreamInspectionNodes.Block,
        [nameof(ParseXmlDocInspections)] = ParseXmlDocInspections.Block,
        [nameof(AccumulateProcessedInspections)] = AccumulateProcessedInspections.Block,
        [nameof(MergeInspections)] = MergeInspections.Block,
        [nameof(BufferInspections)] = BufferInspections.Block,
        [nameof(BroadcastInspections)] = BroadcastInspections.Block,

        [nameof(JoinStagingSources)] = JoinStagingSources.Block,
        [nameof(PrepareStaging)] = PrepareStaging.Block,
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

        StreamAnnotationNodes.CreateBlock(BroadcastXDocument);
        ParseXmlDocAnnotations.CreateBlock(StreamAnnotationNodes);
        AccumulateProcessedAnnotations.CreateBlock(ParseXmlDocAnnotations);
        MergeAnnotations.CreateBlock(() => Context.StagingContext.Annotations, AccumulateProcessedAnnotations.Block.Completion);
        BufferAnnotations.CreateBlock(MergeAnnotations);
        BroadcastAnnotations.CreateBlock(BufferAnnotations);

        StreamQuickFixNodes.CreateBlock(BroadcastXDocument);
        ParseXmlDocQuickFixes.CreateBlock(StreamQuickFixNodes);
        AccumulateProcessedQuickFixes.CreateBlock(ParseXmlDocQuickFixes);
        MergeQuickFixes.CreateBlock(() => Context.StagingContext.QuickFixes, AccumulateProcessedQuickFixes.Block.Completion);
        BufferQuickFixes.CreateBlock(MergeQuickFixes);
        BroadcastQuickFixes.CreateBlock(BufferQuickFixes);

        JoinQuickFixes.CreateBlock(BroadcastXDocument, BroadcastQuickFixes);

        StreamInspectionNodes.CreateBlock(JoinQuickFixes);
        ParseXmlDocInspections.CreateBlock(StreamInspectionNodes);
        AccumulateProcessedInspections.CreateBlock(ParseXmlDocInspections);
        MergeInspections.CreateBlock(() => Context.StagingContext.Inspections, AccumulateProcessedInspections.Block.Completion);
        BufferInspections.CreateBlock(MergeInspections);
        BroadcastInspections.CreateBlock(BufferInspections);

        JoinStagingSources.CreateBlock(BroadcastAnnotations, BroadcastQuickFixes, BroadcastInspections);
        PrepareStaging.CreateBlock(JoinStagingSources);
        SaveStaging.CreateBlock(PrepareStaging);
    }
}
