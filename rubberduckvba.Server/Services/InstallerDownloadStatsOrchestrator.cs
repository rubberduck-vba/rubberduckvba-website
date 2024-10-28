using rubberduckvba.com.Server.ContentSynchronization;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

namespace rubberduckvba.com.Server.Services;

public class InstallerDownloadStatsOrchestrator(ISynchronizationPipelineFactory<SyncContext> factory) : IContentOrchestrator<TagSyncRequestParameters>
{
    public async Task UpdateContentAsync(TagSyncRequestParameters request, CancellationTokenSource tokenSource)
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
