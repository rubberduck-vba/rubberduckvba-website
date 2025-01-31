using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using rubberduckvba.Server;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.Hangfire;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Admin;


[ApiController]
public class AdminController(ConfigurationOptions options, IBackgroundJobClient backgroundJob, ILogger<AdminController> logger) : ControllerBase
{
    /// <summary>
    /// Enqueues a job that updates xmldoc content from the latest release/pre-release tags.
    /// </summary>
    /// <returns>The unique identifier of the enqueued job.</returns>
    [Authorize("github")]
    [HttpPost("admin/update/xmldoc")]
    public async ValueTask<IActionResult> UpdateXmldocContent()
    {
        var parameters = new XmldocSyncRequestParameters { RepositoryId = RepositoryId.Rubberduck, RequestId = Guid.NewGuid() };
        var jobId = backgroundJob.Enqueue(HangfireConstants.ManualQueueName, () => QueuedUpdateOrchestrator.UpdateXmldocContent(parameters, null!));
        logger.LogInformation("JobId {jobId} was enqueued (queue: {queueName}) for xmldoc sync request {requestId}", jobId, HangfireConstants.ManualQueueName, parameters.RequestId);

        return await ValueTask.FromResult(Ok(jobId));
    }

    /// <summary>
    /// Enqueues a job that gets the latest release/pre-release tags and their respective assets, and updates the installer download stats.
    /// </summary>
    /// <returns>The unique identifier of the enqueued job.</returns>
    [Authorize("github")]
    [HttpPost("admin/update/tags")]
    public async ValueTask<IActionResult> UpdateTagMetadata()
    {
        var parameters = new TagSyncRequestParameters { RepositoryId = RepositoryId.Rubberduck, RequestId = Guid.NewGuid() };
        var jobId = backgroundJob.Enqueue(HangfireConstants.ManualQueueName, () => QueuedUpdateOrchestrator.UpdateInstallerDownloadStats(parameters, null!));
        logger.LogInformation("JobId {jobId} was enqueued (queue: {queueName}) for tag sync request {requestId}", jobId, HangfireConstants.ManualQueueName, parameters.RequestId);

        return await ValueTask.FromResult(Ok(jobId));
    }

#if DEBUG
    [HttpGet("admin/config/current")]
    public async ValueTask<IActionResult> Config()
    {
        return await ValueTask.FromResult(Ok(options));
    }
#endif
}

public record class ConfigurationOptions(
    IOptions<ConnectionSettings> ConnectionOptions,
    IOptions<GitHubSettings> GitHubOptions,
    IOptions<HangfireSettings> HangfireOptions)
{
}
