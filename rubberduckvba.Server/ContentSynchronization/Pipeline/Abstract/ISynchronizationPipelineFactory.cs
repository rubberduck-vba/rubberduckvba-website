namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public interface ISynchronizationPipelineFactory<TContext>
    where TContext : class
{
    ISynchronizationPipeline<TContext, bool> Create<TParameters>(TParameters parameters, CancellationTokenSource tokenSource) where TParameters : IRequestParameters;
}
