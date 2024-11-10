namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public interface ISynchronizationPipeline<TContext, TResult> : IPipeline<TContext, TResult>
    where TContext : class
{
    Task<IPipelineResult<TResult>> ExecuteAsync(SyncRequestParameters parameters, CancellationTokenSource tokenSource);
}
