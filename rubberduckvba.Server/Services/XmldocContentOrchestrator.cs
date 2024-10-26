using rubberduckvba.com.Server.ContentSynchronization;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.Services;

public class XmldocContentOrchestrator(ISynchronizationPipelineFactory<SyncContext> factory) : IContentOrchestrator<XmldocSyncRequestParameters>
{
    public async Task UpdateContentAsync(XmldocSyncRequestParameters request, CancellationTokenSource tokenSource)
    {
        var pipeline = factory.Create(request, tokenSource);
        await pipeline.ExecuteAsync(request, tokenSource);
    }
}