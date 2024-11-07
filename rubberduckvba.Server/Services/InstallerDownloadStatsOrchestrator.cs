using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;

namespace rubberduckvba.Server.Services;

public class InstallerDownloadStatsOrchestrator(ISynchronizationPipelineFactory<SyncContext> factory) : IContentOrchestrator<TagSyncRequestParameters>
{
    public async Task UpdateContentAsync(TagSyncRequestParameters request, CancellationTokenSource tokenSource)
    {
        var pipeline = factory.Create(request, tokenSource);
        try
        {
            await pipeline.ExecuteAsync(request, tokenSource);
        }
        catch (TaskCanceledException e)
        {
            var exceptions = pipeline.Exceptions.ToList();
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    throw new OperationCanceledException(e.Message, exceptions[0]);
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
