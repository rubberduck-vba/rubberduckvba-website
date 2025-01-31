using Hangfire;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public class SynchronizationPipelineFactory : ISynchronizationPipelineFactory<SyncContext>
{
    private readonly ILogger _logger;
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;
    private readonly IMarkdownFormattingService _markdown;
    private readonly IRepository<InspectionEntity> _inspections;
    private readonly IRepository<QuickFixEntity> _quickfixes;
    private readonly IRepository<AnnotationEntity> _annotations;

    private readonly TagServices _tagServices;

    private readonly XmlDocAnnotationParser _annotationParser;
    private readonly XmlDocQuickFixParser _quickFixParser;
    private readonly XmlDocInspectionParser _inspectionParser;

    public SynchronizationPipelineFactory(ILogger<PipelineLogger> logger, IRubberduckDbService content, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, IMarkdownFormattingService markdown,
        IRepository<InspectionEntity> inspections, IRepository<QuickFixEntity> quickfixes, IRepository<AnnotationEntity> annotations,
        XmlDocAnnotationParser xmlAnnotationParser, XmlDocQuickFixParser xmlQuickFixParser, XmlDocInspectionParser xmlInspectionParser,
        TagServices tagServices)
    {
        _logger = logger;
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
        _markdown = markdown;
        _inspections = inspections;
        _quickfixes = quickfixes;
        _annotations = annotations;

        _tagServices = tagServices;

        _annotationParser = xmlAnnotationParser;
        _quickFixParser = xmlQuickFixParser;
        _inspectionParser = xmlInspectionParser;
    }

    public ISynchronizationPipeline<SyncContext, bool> Create<TParameters>(TParameters parameters, CancellationTokenSource tokenSource) where TParameters : IRequestParameters
    {
        return parameters switch
        {
            XmldocSyncRequestParameters => new SynchronizeXmlPipeline(parameters, _logger, _content, _github, _merge, _staging, _markdown, tokenSource, _inspections, _quickfixes, _annotations, _annotationParser, _quickFixParser, _inspectionParser, _tagServices),
            TagSyncRequestParameters => new SynchronizeTagsPipeline(parameters, _logger, _content, _github, _merge, _staging, tokenSource),
            _ => throw new NotSupportedException(),
        };
    }
}
public class HangfireTokenSource(IJobCancellationToken token) : CancellationTokenSource
{
    public void ThrowIfCancellationRequested() => token.ThrowIfCancellationRequested();
}
