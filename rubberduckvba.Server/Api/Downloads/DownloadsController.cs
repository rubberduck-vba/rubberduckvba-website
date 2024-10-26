using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.com.Server.Services;
using System.Collections.Immutable;

namespace rubberduckvba.com.Server.Api.Downloads;


[ApiController]
[AllowAnonymous]
public class DownloadsController(IContentCacheService cache, IRubberduckDbService db) : Controller
{
    [HttpGet("downloads")]
    public async Task<ActionResult<IEnumerable<AvailableDownload>>> GetAvailableDownloadsAsync()
    {
        var cacheKey = $"{nameof(GetAvailableDownloadsAsync)}";
        if (!cache.TryGetValue(cacheKey, out IEnumerable<AvailableDownload> downloads))
        {
            var tags = (await db.GetLatestTagsAsync(RepositoryId.Rubberduck)).ToImmutableArray();
            var main = tags[0];
            var next = tags[1];

            var pdfStyleGuide = new AvailableDownload
            {
                Name = "Rubberduck Style Guide (PDF)",
                Title = "Free (pay what you want) PDF download",
                Kind = "pdf",
                DownloadUrl = "https://ko-fi.com/s/d91bfd610c"
            };

            downloads = [
                AvailableDownload.FromTag(main),
                AvailableDownload.FromTag(next),
                pdfStyleGuide
            ];

            cache.SetValue(cacheKey, downloads);
        }

        return downloads.Any() ? Ok(downloads) : NoContent();
    }
}
