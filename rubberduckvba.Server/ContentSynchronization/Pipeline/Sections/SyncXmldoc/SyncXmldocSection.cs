using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;


public class SynchronizeXmlDocSection : PipelineSection<SyncContext>
{
    public SynchronizeXmlDocSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger,
        IRubberduckDbService content,
        IRepository<InspectionEntity> inspections,
        IRepository<QuickFixEntity> quickfixes,
        IRepository<AnnotationEntity> annotations,
        TagServices tagServices,
        IGitHubClientService github,
        IXmlDocMerge mergeService,
        IStagingServices staging,
        XmlDocAnnotationParser xmlAnnotationParser,
        XmlDocQuickFixParser xmlQuickFixParser,
        XmlDocInspectionParser xmlInspectionParser)
        : base(parent, tokenSource, logger)
    {
        Block = new SynchronizeXmlDocBlock(this, tokenSource, logger, content, inspections, quickfixes, annotations, tagServices, github, mergeService, staging, xmlAnnotationParser, xmlQuickFixParser, xmlInspectionParser);
    }

    public SynchronizeXmlDocBlock Block { get; }

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        [nameof(Block)] = Block.Block
    };

    public override void CreateBlocks()
    {
        Block.CreateBlock();
    }
}

public class SynchronizeXmlDocBlock : ActionBlockBase<SyncRequestParameters, SyncContext>
{
    private readonly IRubberduckDbService _content;
    private readonly IRepository<InspectionEntity> _inspections;
    private readonly IRepository<QuickFixEntity> _quickfixes;
    private readonly IRepository<AnnotationEntity> _annotations;
    private readonly TagServices _tagServices;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _mergeService;
    private readonly IStagingServices _staging;
    private readonly XmlDocAnnotationParser _xmlAnnotationParser;
    private readonly XmlDocQuickFixParser _xmlQuickFixParser;
    private readonly XmlDocInspectionParser _xmlInspectionParser;

    public SynchronizeXmlDocBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        IRubberduckDbService content,
        IRepository<InspectionEntity> inspections,
        IRepository<QuickFixEntity> quickfixes,
        IRepository<AnnotationEntity> annotations,
        TagServices tagServices,
        IGitHubClientService github,
        IXmlDocMerge mergeService,
        IStagingServices staging,
        XmlDocAnnotationParser xmlAnnotationParser,
        XmlDocQuickFixParser xmlQuickFixParser,
        XmlDocInspectionParser xmlInspectionParser)
        : base(parent, tokenSource, logger)
    {
        _content = content;
        _inspections = inspections;
        _quickfixes = quickfixes;
        _annotations = annotations;
        _tagServices = tagServices;
        _github = github;
        _mergeService = mergeService;
        _staging = staging;
        _xmlAnnotationParser = xmlAnnotationParser;
        _xmlQuickFixParser = xmlQuickFixParser;
        _xmlInspectionParser = xmlInspectionParser;
    }

    protected override async Task ActionAsync(SyncRequestParameters input)
    {
        Context.LoadParameters(input);

        var dbMain = await _content.GetLatestTagAsync(RepositoryId.Rubberduck, includePreRelease: false);
        Context.LoadRubberduckDbMain(dbMain);

        var githubTags = await _github.GetAllTagsAsync(dbMain.Name);
        // LoadInspectionDefaultConfig
        var config = await _github.GetCodeAnalysisDefaultsConfigAsync();
        Context.LoadInspectionDefaultConfig(config);

        // LoadFeatures
        var inspections = await _content.ResolveFeature(input.RepositoryId, "inspections");
        var quickfixes = await _content.ResolveFeature(input.RepositoryId, "quickfixes");
        var annotations = await _content.ResolveFeature(input.RepositoryId, "annotations");
        Context.LoadFeatures([inspections, quickfixes, annotations]);

        // LoadDbFeatureItems
        await Task.WhenAll([
            Task.Run(() => _inspections.GetAll()).ContinueWith(t => Context.LoadInspections(t.Result.Select(e => new Inspection(e)))),
            Task.Run(() => _quickfixes.GetAll()).ContinueWith(t => Context.LoadQuickFixes(t.Result.Select(e => new QuickFix(e)))),
            Task.Run(() => _annotations.GetAll()).ContinueWith(t => Context.LoadAnnotations(t.Result.Select(e => new Annotation(e))))
        ]);

        // AcquireDbTags
        var ghMain = githubTags.Where(tag => !tag.IsPreRelease).OrderByDescending(tag => tag.DateCreated).ThenByDescending(tag => tag.ReleaseId).Take(1).Single();
        var ghNext = githubTags.Where(tag => tag.IsPreRelease).OrderByDescending(tag => tag.DateCreated).ThenByDescending(tag => tag.ReleaseId).Take(1).Single();

        await Task.Delay(TimeSpan.FromSeconds(2)); // just in case the tags job was scheduled at/around the same time

        var dbNext = await _content.GetLatestTagAsync(RepositoryId.Rubberduck, includePreRelease: true);

        var dbTags = _tagServices.GetAllTags().ToDictionary(e => e.Name);
        List<TagGraph> newTags = [];
        if (ghMain.Name != dbMain.Name)
        {
            if (!dbTags.ContainsKey(ghMain.Name))
            {
                newTags.Add(ghMain);
            }
            else
            {
                // that's an old tag then; do not process
                throw new InvalidOperationException($"Tag metadata mismatch, xmldoc update will not proceed; GitHub@main:{ghMain.Name} ({ghMain.DateCreated}) | rubberduckdb@main: {dbMain.Name} ({dbMain.DateCreated})");
            }
        }
        if (ghNext.Name != dbNext.Name)
        {
            if (!dbTags.ContainsKey(ghMain.Name))
            {
                newTags.Add(ghMain);
            }
            else
            {
                // that's an old tag then; do not process
                throw new InvalidOperationException($"Tag metadata mismatch, xmldoc update will not proceed; GitHub@main:{ghMain.Name} ({ghMain.DateCreated}) | rubberduckdb@main: {dbMain.Name} ({dbMain.DateCreated})");
            }
        }

        _tagServices.Create(newTags);

        Context.LoadRubberduckDbMain(dbMain);
        Context.LoadRubberduckDbNext(dbNext);

        Context.LoadDbTags([dbMain, dbNext]);

        // StreamTagAssets
        var xmldocInfo = new Dictionary<string, Dictionary<Tag, (TagAsset asset, IEnumerable<XElementInfo> nodes)>>()
        {
            [nameof(Annotation)] = [],
            [nameof(QuickFix)] = [],
            [nameof(Inspection)] = []
        };
        foreach (var tag in new[] { dbMain, dbNext })
        {
            foreach (var asset in tag.Assets)
            {
                // DownloadXmlAsset
                if (asset.DownloadUrl is null)
                {
                    Logger.LogWarning(Context.Parameters, "Download url for asset ID {asset.Id} is unexpectedly null.", asset.Id);
                    continue;
                }
                if (Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var uri) && uri.Host != "github.com")
                {
                    Logger.LogWarning(Context.Parameters, $"Unexpected host in download URL '{uri}' from asset ID {asset.Id}");
                    continue;
                }

                using (var client = new HttpClient())
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            Logger.LogInformation(Context.Parameters, $"Loading XDocument from asset {asset.DownloadUrl}...");
                            var document = XDocument.Load(stream, LoadOptions.None);

                            if (asset.Name.Contains("Rubberduck.Parsing", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var annotationNodes = from node in document.Descendants("member").AsParallel()
                                                      let fullName = GetNameOrDefault(node, "Annotation")
                                                      where !string.IsNullOrWhiteSpace(fullName)
                                                      let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                                                      select new XElementInfo(typeName, node);


                                xmldocInfo[nameof(Annotation)].Add(tag, (asset, annotationNodes));
                            }
                            else
                            {
                                var quickFixNodes = from node in document.Descendants("member").AsParallel()
                                                    let fullName = GetNameOrDefault(node, "QuickFix")
                                                    where !string.IsNullOrWhiteSpace(fullName)
                                                    let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                                                    select new XElementInfo(typeName, node);
                                xmldocInfo[nameof(QuickFix)].Add(tag, (asset, quickFixNodes));

                                var inspectionNodes = from node in document.Descendants("member").AsParallel()
                                                      let fullName = GetNameOrDefault(node, "Inspection")
                                                      where !string.IsNullOrWhiteSpace(fullName)
                                                      let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                                                      select new XElementInfo(typeName, node);
                                xmldocInfo[nameof(Inspection)].Add(tag, (asset, inspectionNodes));
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning(Context.Parameters, $"HTTP GET ({uri}) failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}");
                        continue;
                    }
                }
            }

        }

        // parse annotations xmldoc
        var xmlAnnotations = xmldocInfo[nameof(Annotation)];
        foreach (var kvp in xmlAnnotations)
        {
            var (asset, nodes) = kvp.Value;
            foreach (var node in nodes)
            {
                var annotation = _xmlAnnotationParser.Parse(asset.Id, annotations.Id, node.Name, node.Element, kvp.Key.IsPreRelease);
                Context.StagingContext.Annotations.Add(annotation);
            }
        }

        var dbAnnotations = Context.Annotations.ToDictionary(e => e.Name);
        var mergedAnnotations = _mergeService.Merge(dbAnnotations, Context.StagingContext.Annotations.Where(e => !e.IsNew), Context.StagingContext.Annotations.Where(e => e.IsNew));

        // parse quickfix xmldoc
        var xmlQuickFixes = xmldocInfo[nameof(QuickFix)];
        foreach (var kvp in xmlQuickFixes)
        {
            var (asset, nodes) = kvp.Value;
            foreach (var node in nodes)
            {
                var quickfix = _xmlQuickFixParser.Parse(node.Name, asset.Id, quickfixes.Id, node.Element, kvp.Key.IsPreRelease);
                Context.StagingContext.QuickFixes.Add(quickfix);
            }
        }

        var dbQuickfixes = Context.QuickFixes.ToDictionary(e => e.Name);
        var mergedQuickfixes = _mergeService.Merge(dbQuickfixes, Context.StagingContext.QuickFixes.Where(e => !e.IsNew), Context.StagingContext.QuickFixes.Where(e => e.IsNew));
        var unchangedQuickfixes = dbQuickfixes.Values.Where(e => !mergedQuickfixes.Any(q => q.Name == e.Name));

        // parse inspections xmldoc
        var xmlInspections = xmldocInfo[nameof(Inspection)];
        var parseInspections = new List<Task<Inspection>>();
        foreach (var kvp in xmlInspections)
        {
            var (asset, nodes) = kvp.Value;
            foreach (var node in nodes)
            {
                if (!Context.InspectionDefaultConfig.TryGetValue(node.Name, out var defaultConfig))
                {
                    defaultConfig = new InspectionDefaultConfig
                    {
                        DefaultSeverity = "Warning",
                        InspectionType = "CodeQualityIssues",
                        InspectionName = node.Name,
                    };

                    Logger.LogWarning(Context.Parameters, "Default configuration was not found for inspection '{0}'", node.Name);
                }

                var inspection = _xmlInspectionParser.ParseAsync(asset.Id, inspections.Id, mergedQuickfixes.Concat(unchangedQuickfixes), node.Name, node.Element, defaultConfig, kvp.Key.IsPreRelease);
                parseInspections.Add(inspection);
            }
        }

        await Task.WhenAll(parseInspections).ContinueWith(t =>
        {
            foreach (var inspection in t.Result)
            {
                Context.StagingContext.Inspections.Add(inspection);
            }
        });

        var dbInspections = Context.Inspections.ToDictionary(e => e.Name);
        var mergedInspections = _mergeService.Merge(dbInspections, Context.StagingContext.Inspections.Where(e => !e.IsNew), Context.StagingContext.Inspections.Where(e => e.IsNew));

        var staging = new StagingContext(input)
        {
            Annotations = new(mergedAnnotations),
            QuickFixes = new(mergedQuickfixes),
            Inspections = new(mergedInspections)
        };

        await _staging.StageAsync(staging, Token);
    }

    protected static string GetNameOrDefault(XElement memberNode, string suffix)
    {
        var name = memberNode.Attribute("name")?.Value;
        if (name == null || !name.StartsWith("T:") || !name.EndsWith(suffix) || name.EndsWith($"I{suffix}"))
        {
            return default!;
        }

        return name.Substring(2);
    }
}

public class SyncXmldocSection : PipelineSection<SyncContext>
{
    public SyncXmldocSection(IPipeline<SyncContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger,
        IRubberduckDbService content,
        IRepository<InspectionEntity> inspections,
        IRepository<QuickFixEntity> quickfixes,
        IRepository<AnnotationEntity> annotations,
        TagServices tagServices,
        IGitHubClientService github,
        IXmlDocMerge mergeService,
        IStagingServices staging,
        XmlDocAnnotationParser xmlAnnotationParser,
        XmlDocQuickFixParser xmlQuickFixParser,
        XmlDocInspectionParser xmlInspectionParser)
        : base(parent, tokenSource, logger)
    {
        /*
        ReceiveRequest = new ReceiveRequestBlock(this, tokenSource, logger);
        BroadcastParameters = new BroadcastParametersBlock(this, tokenSource, logger);
        LoadInspectionDefaultConfig = new LoadInspectionDefaultConfigBlock(this, tokenSource, github, logger);
        LoadFeatures = new LoadFeaturesBlock(this, tokenSource, content, logger);
        LoadDbFeatureItems = new LoadDbFeatureItemsBlock(this, tokenSource, logger, inspections, quickfixes, annotations);
        AcquireDbMainTagGraph = new AcquireDbMainTagGraphBlock(this, tokenSource, content, logger);
        AcquireDbNextTagGraph = new AcquireDbNextTagGraphBlock(this, tokenSource, content, logger);
        AcquireDbTags = new AcquireDbTagsBlock(this, tokenSource, logger, tagServices);
        JoinDbTags = new DataflowJoinBlock<TagGraph, TagGraph, IEnumerable<Tag>>(this, tokenSource, logger, nameof(JoinDbTags));
        LoadDbTags = new LoadDbTagsBlock(this, tokenSource, logger);
        JoinAsyncSources = new DataflowJoinBlock<SyncContext, SyncContext, SyncContext>(this, tokenSource, logger, nameof(JoinAsyncSources));
        StreamLatestTags = new StreamLatestTagsBlock(this, tokenSource, logger);
        StreamTagAssets = new StreamTagAssetsBlock(this, tokenSource, logger);
        BufferXmlAsset = new XmlTagAssetBufferBlock(this, tokenSource, logger);
        DownloadXmlAsset = new DownloadXmlTagAssetBlock(this, tokenSource, logger);
        BroadcastXDocument = new BroadcastXDocumentBlock(this, tokenSource, logger);
        AcceptInspectionsXDocument = new AcceptInspectionsXDocumentBlock(this, tokenSource, logger);
        //JoinQuickFixes = new DataflowJoinBlock<(TagAsset, XDocument), IEnumerable<QuickFix>>(this, tokenSource, logger, nameof(JoinQuickFixes));
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
        */
        SynchronizeXmlDoc = new SynchronizeXmlDocBlock(this, tokenSource, logger, content, inspections, quickfixes, annotations, tagServices, github, mergeService, staging, xmlAnnotationParser, xmlQuickFixParser, xmlInspectionParser);
    }

    #region blocks
    private SynchronizeXmlDocBlock SynchronizeXmlDoc { get; }
    /*
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

    private AcceptInspectionsXDocumentBlock AcceptInspectionsXDocument { get; }
    //private DataflowJoinBlock<(TagAsset, XDocument), IEnumerable<QuickFix>> JoinQuickFixes { get; }
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
    */
    public ITargetBlock<XmldocSyncRequestParameters> InputBlock => SynchronizeXmlDoc.Block;
    public IDataflowBlock OutputBlock => SynchronizeXmlDoc.Block;

    protected override IReadOnlyDictionary<string, IDataflowBlock> Blocks => new Dictionary<string, IDataflowBlock>
    {
        /*
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

        [nameof(AcceptInspectionsXDocument)] = AcceptInspectionsXDocument.Block,
        //[nameof(JoinQuickFixes)] = JoinQuickFixes.Block,
        [nameof(StreamInspectionNodes)] = StreamInspectionNodes.Block,
        [nameof(ParseXmlDocInspections)] = ParseXmlDocInspections.Block,
        [nameof(AccumulateProcessedInspections)] = AccumulateProcessedInspections.Block,
        [nameof(MergeInspections)] = MergeInspections.Block,
        [nameof(BufferInspections)] = BufferInspections.Block,
        [nameof(BroadcastInspections)] = BroadcastInspections.Block,

        [nameof(JoinStagingSources)] = JoinStagingSources.Block,
        [nameof(PrepareStaging)] = PrepareStaging.Block,
        [nameof(SaveStaging)] = SaveStaging.Block,
        */
        [nameof(SynchronizeXmlDoc)] = SynchronizeXmlDoc.Block,
    };
    #endregion

    public override void CreateBlocks()
    {
        /*
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

        AcceptInspectionsXDocument.CreateBlock(BroadcastXDocument);

        StreamInspectionNodes.CreateBlock(2, () => Context, AcceptInspectionsXDocument.Block.Completion);
        ParseXmlDocInspections.CreateBlock(StreamInspectionNodes);
        AccumulateProcessedInspections.CreateBlock(ParseXmlDocInspections);
        MergeInspections.CreateBlock(() => Context.StagingContext.Inspections, AccumulateProcessedInspections.Block.Completion);
        BufferInspections.CreateBlock(MergeInspections);
        BroadcastInspections.CreateBlock(BufferInspections);

        JoinStagingSources.CreateBlock(BroadcastAnnotations, BroadcastQuickFixes, BroadcastInspections);
        PrepareStaging.CreateBlock(JoinStagingSources);
        SaveStaging.CreateBlock(PrepareStaging);
        */
        SynchronizeXmlDoc.CreateBlock();
    }
}
