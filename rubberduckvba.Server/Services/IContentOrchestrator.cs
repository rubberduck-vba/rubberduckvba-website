using rubberduckvba.com.Server.ContentSynchronization;

namespace rubberduckvba.com.Server.Services;

public interface IContentOrchestrator<TRequest> where TRequest : SyncRequestParameters
{
    Task UpdateContentAsync(TRequest request, CancellationTokenSource tokenSource);
}
