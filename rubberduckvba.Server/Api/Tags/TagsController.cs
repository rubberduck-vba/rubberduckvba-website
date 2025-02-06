using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Tags;


[AllowAnonymous]
public class TagsController : RubberduckApiController
{
    private readonly CacheService cache;
    private readonly IRubberduckDbService db;

    public TagsController(CacheService cache, IRubberduckDbService db, ILogger<TagsController> logger)
        : base(logger)
    {
        this.cache = cache;
        this.db = db;
    }

    /// <summary>
    /// Gets information about the latest release tags.
    /// </summary>
    [HttpGet("api/v1/public/tags")] // legacy route
    [HttpGet("tags/latest")]
    [AllowAnonymous]
    public IActionResult Latest()
    {
        return GuardInternalAction(() =>
        {
            LatestTagsViewModel? result;
            if (!cache.TryGetLatestTags(out result))
            {
                var model = db.GetLatestTagsAsync(RepositoryId.Rubberduck).GetAwaiter().GetResult();
                var main = model.SingleOrDefault(tag => !tag.IsPreRelease);
                var next = model.SingleOrDefault(tag => tag.IsPreRelease);

                if (main != default)
                {
                    result = new LatestTagsViewModel(main, next);
                    cache.Invalidate(result.Value);
                }
            }

            return result is null ? NoContent() : Ok(result);
        });
    }
}
