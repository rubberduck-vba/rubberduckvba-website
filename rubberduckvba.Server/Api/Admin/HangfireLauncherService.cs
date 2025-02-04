using Hangfire;
using rubberduckvba.Server;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.Hangfire;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Admin;

public class HangfireLauncherService(IBackgroundJobClient backgroundJob, ILogger<HangfireLauncherService> logger)
{
    public string UpdateXmldocContent()
    {
        var parameters = new XmldocSyncRequestParameters { RepositoryId = RepositoryId.Rubberduck, RequestId = Guid.NewGuid() };
        var jobId = backgroundJob.Enqueue(HangfireConstants.ManualQueueName, () => QueuedUpdateOrchestrator.UpdateXmldocContent(parameters, null!));

        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new InvalidOperationException("UpdateXmldocContent was requested but enqueueing a Hangfire job did not return a JobId.");
        }
        else
        {
            logger.LogInformation("JobId {jobId} was enqueued (queue: {queueName}) for xmldoc sync request {requestId}", jobId, HangfireConstants.ManualQueueName, parameters.RequestId);
        }

        return jobId;
    }

    public string UpdateTagMetadata()
    {
        var parameters = new TagSyncRequestParameters { RepositoryId = RepositoryId.Rubberduck, RequestId = Guid.NewGuid() };
        var jobId = backgroundJob.Enqueue(HangfireConstants.ManualQueueName, () => QueuedUpdateOrchestrator.UpdateInstallerDownloadStats(parameters, null!));

        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new InvalidOperationException("UpdateXmldocContent was requested but enqueueing a Hangfire job did not return a JobId.");
        }
        else
        {
            logger.LogInformation("JobId {jobId} was enqueued (queue: {queueName}) for tag sync request {requestId}", jobId, HangfireConstants.ManualQueueName, parameters.RequestId);
        }
        return jobId;
    }
}
