using rubberduckvba.com.Server.ContentSynchronization;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.Services;

public class InstallerDownloadStatsOrchestrator(ISynchronizationPipelineFactory<SyncContext> factory) : IContentOrchestrator<TagSyncRequestParameters>
{
    public async Task UpdateContentAsync(TagSyncRequestParameters request, CancellationTokenSource tokenSource)
    {
        var pipeline = factory.Create(request, tokenSource);
        await pipeline.ExecuteAsync(request, tokenSource);
    }
}
