using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server.Services;
using System.Collections.Immutable;

namespace rubberduckvba.Server.Api.Downloads;


[AllowAnonymous]
public class DownloadsController : RubberduckApiController
{
    private readonly CacheService cache;
    private readonly IRubberduckDbService db;

    public DownloadsController(CacheService cache, IRubberduckDbService db, ILogger<DownloadsController> logger)
        : base(logger)
    {
        this.cache = cache;
        this.db = db;
    }

    [HttpGet("downloads")]
    public IActionResult GetAvailableDownloadsAsync()
    {
        return GuardInternalAction(() =>
        {
            AvailableDownload[] result = [];
            if (!cache.TryGetAvailableDownloads(out var cached))
            {
                var tags = db.GetLatestTagsAsync(RepositoryId.Rubberduck).GetAwaiter().GetResult().ToImmutableArray();
                var main = tags[0];
                var next = tags[1];

                var pdfStyleGuide = new AvailableDownload
                {
                    Name = "Rubberduck Style Guide (PDF)",
                    Title = "Free (pay what you want) PDF download",
                    Kind = "pdf",
                    DownloadUrl = "https://ko-fi.com/s/d91bfd610c"
                };

                result =
                [
                    AvailableDownload.FromTag(main),
                    AvailableDownload.FromTag(next),
                    pdfStyleGuide
                ];

                cache.Invalidate(result);
            }
            else
            {
                result = cached ?? [];
            }

            return result.Any() ? Ok(result) : NoContent();
        });
    }
}
