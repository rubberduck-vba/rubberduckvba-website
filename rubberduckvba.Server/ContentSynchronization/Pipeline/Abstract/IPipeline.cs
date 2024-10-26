using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public interface IPipeline<TContext, TResult>
{
    TContext Context { get; }
    IPipelineResult<TResult> Result { get; }

    void Start<TInput>(ITargetBlock<TInput> entryBlock, TInput input);
}