using rubberduckvba.com.Server.ContentSynchronization;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.Services;

public class XmldocContentOrchestrator(ISynchronizationPipelineFactory<SyncContext> factory) : IContentOrchestrator<XmldocSyncRequestParameters>
{
    public async Task UpdateContentAsync(XmldocSyncRequestParameters request, CancellationTokenSource tokenSource)
    {
        var pipeline = factory.Create(request, tokenSource);
        try
        {
            await pipeline.ExecuteAsync(request, tokenSource);
        }
        catch (TaskCanceledException)
        {
            var exceptions = pipeline.Exceptions.ToList();
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    throw exceptions[0];
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }
            throw;
        }
    }
}