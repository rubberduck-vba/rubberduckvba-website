using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline;

public class SynchronizeXmlPipeline : PipelineBase<SyncContext, bool>, ISynchronizationPipeline<SyncContext, bool>
{
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;
    private readonly IMarkdownFormattingService _markdown;
    private readonly IRepository<InspectionEntity> _inspections;
    private readonly IRepository<QuickFixEntity> _quickfixes;
    private readonly IRepository<AnnotationEntity> _annotations;

    public SynchronizeXmlPipeline(IRequestParameters parameters, ILogger logger, IRubberduckDbService content, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, IMarkdownFormattingService markdown, CancellationTokenSource tokenSource,
        IRepository<InspectionEntity> inspections, IRepository<QuickFixEntity> quickfixes, IRepository<AnnotationEntity> annotations)
        : base(new SyncContext(parameters), tokenSource, logger)
    {
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
        _markdown = markdown;
        _inspections = inspections;
        _quickfixes = quickfixes;
        _annotations = annotations;
    }

    public async Task<IPipelineResult<bool>> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource)
    {
        if (_isDisposed)
        {
            // let's not go there
            throw new ObjectDisposedException(nameof(SynchronizeXmlPipeline));
        }

        // 01. Create the pipeline sections
        var synchronizeFeatureItems = new SyncXmldocSection(this, tokenSource, Logger, _content, _inspections, _quickfixes, _annotations, _github, _merge, _staging, _markdown);

        // 02. Wire up the pipeline
        AddSections(parameters, synchronizeFeatureItems);
        DisposeAfter(synchronizeFeatureItems.WhenAllBlocksCompleted);

        // 03. Light it up
        var xmldocRequest = parameters as XmldocSyncRequestParameters;
        Start(synchronizeFeatureItems.InputBlock!, xmldocRequest);

        // 04. await completion
        await synchronizeFeatureItems.OutputBlock.Completion;

        return Result;
    }
}
