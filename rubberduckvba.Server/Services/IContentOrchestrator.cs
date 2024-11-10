using rubberduckvba.Server.ContentSynchronization;

namespace rubberduckvba.Server.Services;

public interface IContentOrchestrator<TRequest> where TRequest : SyncRequestParameters
{
    Task UpdateContentAsync(TRequest request, CancellationTokenSource tokenSource);
}
