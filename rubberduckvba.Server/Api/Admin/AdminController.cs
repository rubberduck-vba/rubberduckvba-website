using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Admin;

[ApiController]
[EnableCors(CorsPolicies.AllowAll)]
public class AdminController(ConfigurationOptions options, HangfireLauncherService hangfire, CacheService cache, IAuditService audits) : ControllerBase
{
    /// <summary>
    /// Enqueues a job that updates xmldoc content from the latest release/pre-release tags.
    /// </summary>
    /// <returns>The unique identifier of the enqueued job.</returns>
    [Authorize("github")]
    [HttpPost("admin/update/xmldoc")]
    public IActionResult UpdateXmldocContent()
    {
        var jobId = hangfire.UpdateXmldocContent();
        return Ok(jobId);
    }

    /// <summary>
    /// Enqueues a job that gets the latest release/pre-release tags and their respective assets, and updates the installer download stats.
    /// </summary>
    /// <returns>The unique identifier of the enqueued job.</returns>
    [Authorize("github")]
    [HttpPost("admin/update/tags")]
    public IActionResult UpdateTagMetadata()
    {
        var jobId = hangfire.UpdateTagMetadata();
        return Ok(jobId);
    }

    [Authorize("github")]
    [HttpPost("admin/cache/clear")]
    public IActionResult ClearCache()
    {
        cache.Clear();
        return Ok();
    }

    [Authorize("github")]
    [HttpGet("admin/audits")]
    public async Task<IActionResult> GetPendingAudits()
    {
        var edits = await audits.GetPendingItems<FeatureEditEntity>();
        var ops = await audits.GetPendingItems<FeatureOpEntity>();

        return Ok(new { edits = edits.ToArray(), other = ops.ToArray() });
    }

#if DEBUG
    [AllowAnonymous]
    [HttpGet("admin/config/current")]
    public IActionResult Config()
    {
        return Ok(options);
    }
#endif
}

public record class ConfigurationOptions(
    IOptions<ConnectionSettings> ConnectionOptions,
    IOptions<GitHubSettings> GitHubOptions,
    IOptions<HangfireSettings> HangfireOptions)
{
}
