using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public interface IPipelineResult<TResult>
{
    TResult Result { get; set; }
    IEnumerable<Exception> Exceptions { get; }

    void AddException(Exception exception);
}

public abstract class PipelineSection<TContext> : PipelineBase<TContext, bool>
    where TContext : IPipelineContext
{
    protected PipelineSection(IPipeline<TContext, bool> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected abstract IReadOnlyDictionary<string, IDataflowBlock> Blocks { get; }
    public abstract void CreateBlocks();

    public Exception? Exception { get; private set; }

    private readonly List<Task> _tracedCompletionTasks = [];
    public void TraceBlockCompletion(IDataflowBlock block, string name)
    {
        _tracedCompletionTasks.Add(block.Completion.ContinueWith(t =>
        {
            string status;
            switch (t.Status)
            {
                case TaskStatus.Canceled:
                    status = "task is canceled.";
                    break;
                case TaskStatus.Faulted:
                    status = "task is faulted.";
                    break;
                case TaskStatus.RanToCompletion:
                    status = "task ran to completion.";
                    break;
                default:
                    status = $"task is in unexpected state {t.Status}.";
                    break;
            }
            Logger.LogTrace(Context.Parameters, $"{GetType().Name} | ✅ Dataflow block completion task completed | {name} ({block.GetType().Name}) | {status}");
        })
            .ContinueWith(t =>
            {
                var details = Blocks.Select(
                    block => $"{(block.Value.Completion.IsCompletedSuccessfully ? "✔️" :
                                 block.Value.Completion.IsFaulted ? "✖️" :
                                 block.Value.Completion.IsCanceled ? "⛔" : "🕑")} {block.Key} : {block.Value.Completion.Status}");
                Logger.LogTrace(Context.Parameters, "Pipeline block completion status details" + Environment.NewLine + string.Join(Environment.NewLine, details));
            }));
    }

    public void FaultPipelineBlock(IDataflowBlock block, Exception exception)
    {
        if (Exception is null)
        {
            Exception = exception;
            Result.AddException(exception);

            block.Fault(exception);
            Logger.LogWarning(Context.Parameters, $"{GetType().Name} | ⚠️ Block ({block.GetType().Name}) was faulted");

            try
            {
                Logger.LogWarning(Context.Parameters, $"{GetType().Name} |  ⚠️ Cancelling token source");
                TokenSource?.Cancel();
            }
            // cancellation exception bubbles up
            catch (ObjectDisposedException)
            {
                Logger.LogWarning(Context.Parameters, $"{GetType().Name} |  ⚠️ Token source disposed: cancellation already happened");
            }
        }
    }

    protected void ThrowIfCancellationRequested() => Token.ThrowIfCancellationRequested();

    /// <summary>
    /// Gets a task that completes when all blocks in the section have completed.
    /// </summary>
    public Task WhenAllBlocksCompleted => Task.WhenAll(Blocks.Values.Select(block => block.Completion));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in Blocks)
            {
                if (item.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _tracedCompletionTasks.Clear();
        }
    }
}