using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class ExecutionDataflowBlockBase<TBlock, TInput, TContext> : DataflowBlockBase<TBlock, TContext>, IDisposable
    where TBlock : class, IDataflowBlock, ITargetBlock<TInput>
    where TContext : IPipelineContext
{
    private readonly ICollection<IDisposable> _links = new List<IDisposable>();

    protected ExecutionDataflowBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public TBlock Block { get; private set; } = default!;

    public override IDataflowBlock? TryGetBlock() => Block;

    public TBlock CreateBlock()
    {
        return CreateBlock(parallelism: 1);
    }
    public TBlock CreateBlock(int parallelism)
    {
        return CreateBlock(parallelism, Array.Empty<ISectionBlock<TContext>>());
    }

    public TBlock CreateBlock(params ISourceBlock<TInput>[] sources)
    {
        return CreateBlock(1, sources);
    }
    public TBlock CreateBlock(params ISectionBlock<TContext>[] sources)
    {
        return CreateBlock(1, sources);
    }

    public TBlock CreateBlock(Func<TInput> source, params Task[] waitForTasks)
    {
        return CreateBlock(1, source, waitForTasks);
    }

    public TBlock CreateBlock(int parallelism, params ISourceBlock<TInput>[] sources)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            CancellationToken = Token,
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = 1
        };
        return CreateBlock(options, sources);
    }

    public TBlock CreateBlock(int parallelism, params ISectionBlock<TContext>[] sources)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            CancellationToken = Token,
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = 1
        };
        return CreateBlock(options, sources);
    }

    public TBlock CreateBlock(int parallelism, Func<TInput> source, params Task[] waitForTasks)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            CancellationToken = Token,
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = 1
        };
        return CreateBlock(options, source, waitForTasks);
    }

    public TBlock CreateBlock(ExecutionDataflowBlockOptions options, params ISourceBlock<TInput>[] sources)
    {
        Logger.LogTrace(Context.Parameters, $"{Name} | 🧩 Creating pipeline block...");
        Block = CreateBlock(options);

        if (sources != null)
        {
            LinkToSources(sources);
        }
        Parent.TraceBlockCompletion(Block, Name);

        return Block;
    }

    public TBlock CreateBlock(ExecutionDataflowBlockOptions options, params ISectionBlock<TContext>[] sources)
    {
        Logger.LogTrace(Context.Parameters, $"{Name} | 🧩 Creating pipeline block...");
        Block = CreateBlock(options);

        if (sources != null)
        {
            LinkToSources(sources);
        }
        Parent.TraceBlockCompletion(Block, Name);

        return Block;
    }

    public TBlock CreateBlock(ExecutionDataflowBlockOptions options, Func<TInput> source, params Task[] waitForTasks)
    {
        Logger.LogTrace(Context.Parameters, $"{Name} | 🧩 Creating pipeline block...");
        Block = CreateBlock(options);

        if (waitForTasks?.Any() ?? false)
        {
            LinkFromSource(source, waitForTasks);
        }
        Parent.TraceBlockCompletion(Block, Name);

        return Block;
    }

    public abstract TBlock CreateBlock(ExecutionDataflowBlockOptions options);

    protected void LinkToSources(params ISourceBlock<TInput>[] sources)
    {
        if (sources != null && sources.Length > 0)
        {
            var propagate = sources.Count() == 1;

            foreach (var source in sources)
            {
                var propagateSourceCompletion = propagate
                    && source is not BroadcastBlock<TInput>;

                _links.Add(source.LinkTo(Block, new DataflowLinkOptions { PropagateCompletion = propagateSourceCompletion }));
                Logger.LogTrace(Context.Parameters, $"{Name} | 🔗 Linked to {source.GetType().Name} source block | Propagation: {(propagateSourceCompletion ? "ON" : "OFF")}");
            }

            WaitAllTasks(sources.Select(source => source.Completion).ToArray());
        }
    }

    protected void LinkToSources(params ISectionBlock<TContext>[] sources)
    {
        if (sources != null && sources.Length > 0)
        {
            var propagate = sources.Count(e => e.TryGetBlock() is ISourceBlock<TInput>) == 1
                && !sources.OfType<BroadcastBlockBase<TInput, TContext>>().Any();

            var completionTasks = new List<Task>();

            if (Block is ITargetBlock<TInput> targetBlock)
            {
                foreach (var source in sources)
                {
                    var srcBlock = source.TryGetBlock();

                    if (srcBlock is ISourceBlock<TInput> sourceBlock)
                    {
                        _links.Add(sourceBlock.LinkTo(targetBlock, new DataflowLinkOptions { PropagateCompletion = propagate }));
                        Logger.LogTrace(Context.Parameters, $"{Name} | 🔗 Linked to {source.Name} source block | Propagation: {(propagate ? "ON" : "OFF")}");

                        if (!propagate)
                        {
                            completionTasks.Add(sourceBlock.Completion);
                        }
                    }
                    else if (srcBlock != null)
                    {
                        //completionTasks.Add(srcBlock.Completion);
                        Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Source block ({srcBlock.GetType().Name}) is not ISourceBlock<{typeof(TInput).Name}>. Pipeline may not complete.");
                        throw new InvalidOperationException($"Source block ({srcBlock.GetType().Name}) is not ISourceBlock<{typeof(TInput).Name}>");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Source block is not defined.");
                    }
                }
            }

            WaitAllTasks(completionTasks.ToArray());
        }
    }

    private List<Task> _whenAllTasks = [];

    protected void WaitAllTasks(params Task[] completionTasks)
    {
        if (completionTasks?.Any() ?? false)
        {
            Logger.LogTrace(Context.Parameters, $"{Name} | 🕑 Awaiting the completion of {completionTasks.Length} source block task{(completionTasks.Length > 1 ? "s" : string.Empty)}");

            var sw = Stopwatch.StartNew();
            _whenAllTasks.Add(Task.WhenAll(completionTasks).ContinueWith(t =>
            {
                sw.Stop();
                Block.Complete();
                Logger.LogInformation(Context.Parameters, $"{Name} | ☑️ Block completed | ⏱️ {sw.Elapsed}");
                Parent.LogBlockCompletionDetails();
            }));
        }
    }

    protected void LinkFromSource(Func<TInput> blockOutput, params Task[] waitForTasks)
    {
        if (waitForTasks?.Any() ?? false)
        {
            Logger.LogTrace(Context.Parameters, $"{Name} | 🕑 Awaiting the completion of {waitForTasks.Length} source block task{(waitForTasks.Length > 1 ? "s" : string.Empty)}");

            _whenAllTasks.Add(Task.WhenAll(waitForTasks).ContinueWith(t =>
            {
                try
                {
                    var output = blockOutput.Invoke();
                    Logger.LogTrace(Context.Parameters, $"{Name} | ✔️ Received source output ({typeof(TInput).Name})");

                    if (!Block.Post(output))
                    {
                        throw new InvalidOperationException($"Input ({typeof(TInput).Name}) was not accepted");
                    }

                    Logger.LogTrace(Context.Parameters, $"{Name} | 🚀 Block accepted input ({typeof(TInput).Name})");
                    Block.Complete();
                }
                catch (Exception exception)
                {
                    Logger.LogException(Context.Parameters, exception);
                    throw;
                }
            }, Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current));
        }
    }

    protected void GuardInternalAction(Action<TInput> action, TInput input)
    {
        var sw = Stopwatch.StartNew();
        var success = false;
        try
        {
            Token.ThrowIfCancellationRequested();
            Logger.LogTrace(Context.Parameters, $"{Name} | ▶️ Token cleared, invoking block action");

            action.Invoke(input);
            success = true;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Operation was cancelled");
        }
        catch (Exception exception)
        {
            Logger.LogException(Context.Parameters, exception);
            Parent.FaultPipelineBlock(Block, exception);
        }
        finally
        {
            sw.Stop();
            if (success)
            {
                Logger.LogInformation(Context.Parameters, $"{Name} | ✔️ Block action completed successfully | ⏱️ {sw.Elapsed}");
            }
            else
            {
                Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Block action completed with errors");
            }
        }
    }

    protected TOutput GuardInternalFunc<TOutput>(Func<TInput, TOutput> func, TInput input)
    {
        var sw = Stopwatch.StartNew();
        var success = false;
        try
        {
            Token.ThrowIfCancellationRequested();
            Logger.LogTrace(Context.Parameters, $"{Name}| ▶️ Token cleared, invoking block function");

            var result = func.Invoke(input);

            success = true;

            Logger.LogTrace(Context.Parameters, $"{Name} | ✔️ Block function returned a result ({typeof(TOutput).Name}) | ⏱️ {sw.Elapsed}");
            return result;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Operation was cancelled");
        }
        catch (Exception exception)
        {
            Logger.LogException(Context.Parameters, exception);
            Parent.FaultPipelineBlock(Block, exception);
        }
        finally
        {
            sw.Stop();
            if (success)
            {
                Logger.LogInformation(Context.Parameters, $"{Name} | ✔️ Block function completed successfully | ⏱️ {sw.Elapsed}");
            }
            else
            {
                Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Block function completed with errors");
            }
        }

        if (!Token.IsCancellationRequested)
        {
            Logger.LogWarning(Context.Parameters, $"{Name} | ⚠️ Output is unexpected; verify previous error messages, pipeline is expected to subsequently fail");
        }
        return default!;
    }

    public void Dispose()
    {
        foreach (var link in _links)
        {
            link.Dispose();
        }
        Logger.LogTrace(Context.Parameters, $"{Name} | ✔️ Disposed {_links.Count} links");
        _links.Clear();

        foreach (var task in _whenAllTasks)
        {
            task.Dispose();
        }
        _whenAllTasks.Clear();
    }
}
