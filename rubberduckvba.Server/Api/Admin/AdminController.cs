using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace rubberduckvba.Server.Api.Admin;


[ApiController]
public class AdminController(ConfigurationOptions options, HangfireLauncherService hangfire) : ControllerBase
{
    /// <summary>
    /// Enqueues a job that updates xmldoc content from the latest release/pre-release tags.
    /// </summary>
    /// <returns>The unique identifier of the enqueued job.</returns>
    [Authorize("github")]
    [EnableCors("CorsPolicy")]
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
    [EnableCors("CorsPolicy")]
    [HttpPost("admin/update/tags")]
    public IActionResult UpdateTagMetadata()
    {
        var jobId = hangfire.UpdateTagMetadata();
        return Ok(jobId);
    }

#if DEBUG
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
