using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using System.Security.Principal;

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
    [HttpGet("admin/audits/pending")]
    public async Task<IActionResult> GetPendingAudits()
    {
        var edits = await audits.GetPendingItems<FeatureEditEntity>();
        var ops = await audits.GetPendingItems<FeatureOpEntity>();

        return Ok(new { edits = edits.ToArray(), other = ops.ToArray() });
    }

    [Authorize("github")]
    [HttpGet("admin/audits/{featureId}")]
    public async Task<IActionResult> GetPendingAudits([FromRoute] int featureId)
    {
        var edits = await audits.GetPendingItems<FeatureEditEntity>(featureId);
        var ops = await audits.GetPendingItems<FeatureOpEntity>(featureId);

        return Ok(new { edits = edits.ToArray(), other = ops.ToArray() });
    }

    [Authorize("github")]
    [HttpPost("admin/audits/approve/{id}")]
    public async Task<IActionResult> ApprovePendingAudit([FromRoute] int id)
    {
        if (User.Identity is not IIdentity identity)
        {
            // this is arguably a bug in the authentication middleware, but we can handle it gracefully here
            return Unauthorized("User identity is not available.");
        }

        var edits = await audits.GetPendingItems<FeatureEditEntity>();
        AuditEntity? audit;

        audit = edits.SingleOrDefault(e => e.Id == id);
        if (audit is null)
        {
            var ops = await audits.GetPendingItems<FeatureOpEntity>();
            audit = ops.SingleOrDefault(e => e.Id == id);
        }

        if (audit is null)
        {
            // TODO log this
            return BadRequest("Invalid ID");
        }

        if (!audit.IsPending)
        {
            // TODO log this
            return BadRequest($"This operation has already been audited");
        }

        await audits.Approve(audit, identity);
        return Ok("Operation was approved successfully.");
    }

    [Authorize("github")]
    [HttpPost("admin/audits/reject/{id}")]
    public async Task<IActionResult> RejectPendingAudit([FromRoute] int id)
    {
        if (User.Identity is not IIdentity identity)
        {
            // this is arguably a bug in the authentication middleware, but we can handle it gracefully here
            return Unauthorized("User identity is not available.");
        }

        var edits = await audits.GetPendingItems<FeatureEditEntity>();
        AuditEntity? audit;

        audit = edits.SingleOrDefault(e => e.Id == id);
        if (audit is null)
        {
            var ops = await audits.GetPendingItems<FeatureOpEntity>();
            audit = ops.SingleOrDefault(e => e.Id == id);
        }

        if (audit is null)
        {
            // TODO log this
            return BadRequest("Invalid ID");
        }

        if (!audit.IsPending)
        {
            // TODO log this
            return BadRequest($"This operation has already been audited");
        }

        await audits.Reject(audit, identity);
        return Ok("Operation was rejected successfully.");
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
