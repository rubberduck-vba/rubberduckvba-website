namespace rubberduckvba.Server.Data;


public enum SyncStatus
{
    Received = 0,
    Started = 1,
    Success = 2,
    Error = -1
}

public record class SynchronizationRequest
{
    public Guid RequestId { get; init; } = default!;
    public string JobId { get; init; } = default!;
    public DateTime UtcDateTimeStarted { get; init; } = default!;
    public DateTime? UtcDateTimeEnded { get; init; } = default!;
    public SyncStatus Status { get; init; } = default!;
    public string Message { get; init; } = default!;
}

public static class SynchronizationMessage
{
    public static string FromStatus(SyncStatus status) => status switch
    {
        SyncStatus.Received => "Synchronization queued.",
        SyncStatus.Started => "Synchronization started.",
        SyncStatus.Success => "Synchronization completed.",
        SyncStatus.Error => "Synchronization failed.",
        _ => "(unknown status)"
    };
}
