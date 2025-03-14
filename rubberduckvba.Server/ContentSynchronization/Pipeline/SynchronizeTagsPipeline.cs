using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline;

public class SynchronizeTagsPipeline : PipelineBase<SyncContext, bool>, ISynchronizationPipeline<SyncContext, bool>
{
    private readonly IRubberduckDbService _content;
    private readonly IGitHubClientService _github;
    private readonly IXmlDocMerge _merge;
    private readonly IStagingServices _staging;
    private readonly TagServices _tagServices;

    public SynchronizeTagsPipeline(IRequestParameters parameters, ILogger logger,
        IRubberduckDbService content, TagServices tagServices, IGitHubClientService github, IXmlDocMerge merge, IStagingServices staging, CancellationTokenSource tokenSource)
        : base(new SyncContext(parameters), tokenSource, logger)
    {
        _content = content;
        _github = github;
        _merge = merge;
        _staging = staging;
        _tagServices = tagServices;
    }

    public async Task<IPipelineResult<bool>> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource)
    {
        if (_isDisposed)
        {
            // let's not go there
            throw new ObjectDisposedException(nameof(SynchronizeXmlPipeline));
        }

        // 01. Create the pipeline sections
        var synchronizeTags = new SyncTagsSection(this, tokenSource, Logger, _tagServices, _github, _staging);

        // 02. Wire up the pipeline
        AddSections(parameters, synchronizeTags);
        DisposeAfter(synchronizeTags.WhenAllBlocksCompleted);

        // 03. Light it up
        Start(synchronizeTags.InputBlock, (TagSyncRequestParameters)parameters);

        // 04. await completion
        await synchronizeTags.OutputTask;

        return Result;
    }
}
