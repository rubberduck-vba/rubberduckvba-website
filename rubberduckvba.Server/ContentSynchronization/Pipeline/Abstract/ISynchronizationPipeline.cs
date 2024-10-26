namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public interface ISynchronizationPipeline<TContext>
    where TContext : class
{
    TContext Context { get; }
    Task<TContext> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource);
}
