using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Tags;


[ApiController]
[AllowAnonymous]
public class TagsController(IRubberduckDbService db) : ControllerBase
{
    /// <summary>
    /// Gets information about the latest release tags.
    /// </summary>
    [HttpGet("tags/latest")]
    [AllowAnonymous]
    public async Task<ActionResult<LatestTagsViewModel>> Latest()
    {
        var model = await db.GetLatestTagsAsync(RepositoryId.Rubberduck);
        var main = model.SingleOrDefault(tag => !tag.IsPreRelease);
        var next = model.SingleOrDefault(tag => tag.IsPreRelease);

        if (main == default)
        {
            return NoContent();
        }

        var tags = new LatestTagsViewModel(main, next);
        return Ok(tags);
    }
}
