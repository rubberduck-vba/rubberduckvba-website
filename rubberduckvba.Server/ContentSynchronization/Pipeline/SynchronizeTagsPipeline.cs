using System.Threading.Tasks.Dataflow;

using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline;

public class SynchronizeTagsPipeline : PipelineBase<SyncContext, bool>, ISynchronizationPipeline<SyncContext>
{
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;

    public SynchronizeTagsPipeline(IRequestParameters parameters, ILogger logger, IRubberduckDbService content, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, CancellationTokenSource tokenSource)
        : base(new SyncContext(parameters), tokenSource, logger)
    {
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
    }

    public async Task<SyncContext> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource)
    {
        if (_isDisposed)
        {
            // let's not go there
            throw new ObjectDisposedException(nameof(SynchronizeXmlPipeline));
        }

        // 01. Create the pipeline sections
        var synchronizeTags = new SyncTagsSection(this, tokenSource, Logger, _content, _github, _staging);

        // 02. Wire up the pipeline
        AddSections(parameters, synchronizeTags);
        DisposeAfter(synchronizeTags.WhenAllBlocksCompleted);

        // 03. Light it up
        Start(synchronizeTags.InputBlock, parameters);

        // 04. await completion
        return await synchronizeTags.OutputTask.ContinueWith(t => Context, tokenSource.Token);
    }
}
