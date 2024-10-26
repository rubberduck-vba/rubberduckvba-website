using Hangfire;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public class SynchronizationPipelineFactory : ISynchronizationPipelineFactory<SyncContext>
{
    private readonly ILogger _logger;
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;
    private readonly IMarkdownFormattingService _markdown;

    public SynchronizationPipelineFactory(ILogger<PipelineLogger> logger, IRubberduckDbService content, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, IMarkdownFormattingService markdown)
    {
        _logger = logger;
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
        _markdown = markdown;
    }

    public ISynchronizationPipeline<SyncContext> Create<TParameters>(TParameters parameters, CancellationTokenSource tokenSource) where TParameters : IRequestParameters
    {
        return parameters switch
        {
            XmldocSyncRequestParameters => new SynchronizeXmlPipeline(parameters, _logger, _content, _github, _merge, _staging, _markdown, tokenSource),
            TagSyncRequestParameters => new SynchronizeTagsPipeline(parameters, _logger, _content, _github, _merge, _staging, tokenSource),
            _ => throw new NotSupportedException(),
        };
    }
}

public class HangfireTokenSource(IJobCancellationToken token) : CancellationTokenSource
{
    public void ThrowIfCancellationRequested() => token.ThrowIfCancellationRequested();
}