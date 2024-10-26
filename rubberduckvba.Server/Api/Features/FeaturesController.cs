using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace rubberduckvba.com.Server.Api.Features;

public record class MarkdownFormattingRequestViewModel
{
    public string MarkdownContent { get; init; }
    public bool WithVbeCodeBlocks { get; init; }
}


[ApiController]
[AllowAnonymous]
public class FeaturesController(IRubberduckDbService db, IMarkdownFormattingService md, ICacheService cache) : ControllerBase
{
    private static RepositoryOptionViewModel[] RepositoryOptions { get; } =
        Enum.GetValues<RepositoryId>().Select(e => new RepositoryOptionViewModel { Id = e, Name = e.ToString() }).ToArray();

    private async Task<FeatureOptionViewModel[]> GetFeatureOptions(RepositoryId repositoryId) =>
        await db.GetTopLevelFeatures(repositoryId)
            .ContinueWith(t => t.Result.Select(e => new FeatureOptionViewModel { Id = e.Id, Name = e.Name, Title = e.Title }).ToArray());

    [HttpGet("features")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FeatureViewModel>>> Index()
    {
        //if (cache.TryGet<FeatureViewModel[]>(HttpContext.Request.Path, out var cached))
        //{
        //    return cached;
        //}

        var features = await db.GetTopLevelFeatures(RepositoryId.Rubberduck);
        if (!features.Any())
        {
            return NoContent();
        }

        var model = features.Select(feature => new FeatureViewModel(feature)).ToArray();
        //cache.Write(HttpContext.Request.Path, model);

        return Ok(model);
    }

    [HttpGet("features/{name}")]
    [AllowAnonymous]
    public async Task<ActionResult<FeatureViewModel>> Info([FromRoute] string name)
    {
        //if (cache.TryGet<FeatureViewModel>(HttpContext.Request.Path, out var cached))
        //{
        //    return cached;
        //}

        var feature = await db.ResolveFeature(RepositoryId.Rubberduck, name);
        if (feature is null)
        {
            return NotFound();
        }

        var moduleTypeNames = typeof(ExampleModuleType).GetMembers().Where(e => e.GetCustomAttribute<DescriptionAttribute>() != null).ToDictionary(member => member.Name, member => member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? member.Name);

        var model = new FeatureViewModel(feature with
        {
            Description = await md.FormatMarkdownDocument(feature.Description, true),
            ShortDescription = feature.ShortDescription,

            ParentId = feature.Id,
            ParentName = feature.Name,
            ParentTitle = feature.Title,

            Features = feature.Features.Select(subFeature => subFeature with
            {
                Description = subFeature.Description,
                ShortDescription = subFeature.ShortDescription
            }).ToArray(),

            Inspections = feature.Items.Select(item =>
            {
                var info = JsonSerializer.Deserialize<InspectionProperties>(item.Serialized)!;
                return new XmlDocInspectionInfo
                {
                    Id = item.Id,
                    DateTimeInserted = item.DateTimeInserted,
                    DateTimeUpdated = item.DateTimeUpdated,
                    IsDiscontinued = item.IsDiscontinued,
                    IsHidden = item.IsHidden,
                    IsNew = item.IsNew,
                    Name = item.Name,
                    Title = item.Title,
                    SourceUrl = item.SourceUrl,

                    FeatureId = feature.Id,
                    FeatureName = feature.Name,
                    FeatureTitle = feature.Title,

                    Summary = info.Summary,
                    Reasoning = info.Reasoning,
                    Remarks = info.Remarks,
                    HostApp = info.HostApp,
                    References = info.References,
                    InspectionType = info.InspectionType,
                    DefaultSeverity = info.DefaultSeverity,
                    QuickFixes = info.QuickFixes,
                    Serialized = item.Serialized,
                    Examples = info.Examples ?? []
                };
            }).ToArray(),

            Items = feature.Items
        });

        //cache.Write(HttpContext.Request.Path, model);
        return Ok(model);
    }

    [HttpGet("features/resolve")]
    [AllowAnonymous]
    public async Task<ActionResult> Resolve([FromQuery] RepositoryId repository, [FromQuery] string name)
    {
        var graph = await db.ResolveFeature(repository, name);
        var markdown = await md.FormatMarkdownDocument(graph.Description, withSyntaxHighlighting: true);
        return Ok(graph with { Description = markdown });
    }

    [HttpGet("features/create")]
    //[Authorize("github")]
    public async Task<ActionResult<FeatureEditViewModel>> Create([FromQuery] RepositoryId repository = RepositoryId.Rubberduck, [FromQuery] int? parentId = default)
    {
        var features = await GetFeatureOptions(repository);
        var parent = features.SingleOrDefault(e => e.Id == parentId);

        var model = FeatureEditViewModel.Default(repository, parent, features, RepositoryOptions);
        return Ok(model);
    }

    [HttpPost("create")]
    //[Authorize("github")]
    public async Task<ActionResult<FeatureEditViewModel>> Create([FromBody] FeatureEditViewModel model)
    {
        if (model.Id.HasValue || string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length < 3)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var existing = await db.ResolveFeature(model.RepositoryId, model.Name);
        if (existing != null)
        {
            return BadRequest($"Model [Name] must be unique; feature '{model.Name}' already exists.");
        }

        var feature = model.ToFeature();
        var result = await db.SaveFeature(feature);

        var features = await GetFeatureOptions(model.RepositoryId);
        return Ok(new FeatureEditViewModel(result, features, RepositoryOptions));
    }

    [HttpPost("features/update")]
    //[Authorize("github")]
    public async Task<ActionResult<FeatureEditViewModel>> Update([FromBody] FeatureEditViewModel model)
    {
        if (model.Id.GetValueOrDefault() == default)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var existing = await db.ResolveFeature(model.RepositoryId, model.Name);
        if (existing is null)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var result = await db.SaveFeature(model.ToFeature());
        var features = await GetFeatureOptions(model.RepositoryId);

        return new FeatureEditViewModel(result, features, RepositoryOptions);
    }

    [HttpPost("features/markdown")]
    public async Task<ActionResult> FormatMarkdown([FromBody] MarkdownFormattingRequestViewModel model)
    {
        return Ok(await md.FormatMarkdownDocument(model.MarkdownContent, model.WithVbeCodeBlocks));
    }
}
