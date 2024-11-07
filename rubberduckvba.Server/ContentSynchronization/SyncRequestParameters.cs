using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization;

public interface IRequestParameters
{
    Guid RequestId { get; }
    string? JobId { get; }
}

public record class SyncRequestParameters : IRequestParameters
{
    public Guid RequestId { get; init; }
    public string? JobId { get; init; }

    public RepositoryId RepositoryId { get; init; }

    public string? Token { get; init; }
}

public record class XmldocSyncRequestParameters : SyncRequestParameters
{
}

public record class TagSyncRequestParameters : SyncRequestParameters
{
    public string? Tag { get; init; }
}
