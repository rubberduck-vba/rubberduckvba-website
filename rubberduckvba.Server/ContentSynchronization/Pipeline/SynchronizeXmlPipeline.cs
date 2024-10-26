using System.Threading.Tasks.Dataflow;

using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline;

public class SynchronizeXmlPipeline : PipelineBase<SyncContext, bool>, ISynchronizationPipeline<SyncContext>
{
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;
    private readonly IMarkdownFormattingService _markdown;

    public SynchronizeXmlPipeline(IRequestParameters parameters, ILogger logger, IRubberduckDbService content, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, IMarkdownFormattingService markdown, CancellationTokenSource tokenSource)
        : base(new SyncContext(parameters), tokenSource, logger)
    {
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
        _markdown = markdown;
    }

    public async Task<SyncContext> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource)
    {
        if (_isDisposed)
        {
            // let's not go there
            throw new ObjectDisposedException(nameof(SynchronizeXmlPipeline));
        }

        // 01. Create the pipeline sections
        var synchronizeFeatureItems = new SyncXmldocSection(this, tokenSource, Logger, _content, _github, _merge, _staging, _markdown);

        // 02. Wire up the pipeline
        AddSections(parameters, synchronizeFeatureItems);
        DisposeAfter(synchronizeFeatureItems.WhenAllBlocksCompleted);

        // 03. Light it up
        var xmldocRequest = parameters as XmldocSyncRequestParameters;
        Start(synchronizeFeatureItems.InputBlock!, xmldocRequest);

        // 04. await completion
        return await synchronizeFeatureItems.OutputBlock.Completion.ContinueWith(t => Context);
    }
}
