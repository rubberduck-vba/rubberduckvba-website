using System.Collections.Concurrent;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public class PipelineResult<TResult> : IPipelineResult<TResult>
{
    private readonly ConcurrentBag<Exception> _exceptions = new ConcurrentBag<Exception>();

    public TResult Result { get; set; }
    public IEnumerable<Exception> Exceptions => _exceptions;

    public void AddException(Exception exception)
    {
        _exceptions.Add(exception);
    }
}