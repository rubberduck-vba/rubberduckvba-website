using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;
using System.ComponentModel;
using System.Reflection;

namespace rubberduckvba.Server.Api.Features;

public record class MarkdownFormattingRequestViewModel
{
    public string MarkdownContent { get; init; }
    public bool WithVbeCodeBlocks { get; init; }
}


[ApiController]
[AllowAnonymous]
public class FeaturesController(IRubberduckDbService db, FeatureServices features, FeatureViewModelFactory vmFactory, IMarkdownFormattingService md, ICacheService cache) : ControllerBase
{
    private static RepositoryOptionViewModel[] RepositoryOptions { get; } =
        Enum.GetValues<RepositoryId>().Select(e => new RepositoryOptionViewModel { Id = e, Name = e.ToString() }).ToArray();

    private async Task<FeatureOptionViewModel[]> GetFeatureOptions(RepositoryId repositoryId) =>
        await db.GetTopLevelFeatures(repositoryId)
            .ContinueWith(t => t.Result.Select(e => new FeatureOptionViewModel { Id = e.Id, Name = e.Name, Title = e.Title }).ToArray());

    private bool TryGetCachedContent(out string key, out object cached)
    {
        key = HttpContext.Request.Path;
        return cache.TryGet(key, out cached);
    }

    [HttpGet("features")]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        //if (TryGetCachedContent(out var key, out var cached))
        //{
        //    return Ok(cached);
        //}

        var features = await db.GetTopLevelFeatures(RepositoryId.Rubberduck);
        if (!features.Any())
        {
            return NoContent();
        }

        var model = vmFactory.Create(features);
        //cache.Write(key, model);

        return Ok(model);
    }

    private static readonly IDictionary<string, string> _moduleTypeNames = typeof(ExampleModuleType).GetMembers().Where(e => e.GetCustomAttribute<DescriptionAttribute>() != null)
        .ToDictionary(member => member.Name, member => member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? member.Name);

    [HttpGet("features/{name}")]
    [AllowAnonymous]
    public IActionResult Info([FromRoute] string name)
    {
        if (TryGetCachedContent(out var key, out var cached))
        {
            return Ok(cached);
        }

        var feature = features.Get(name);
        if (feature is null)
        {
            return NotFound();
        }

        var model = vmFactory.Create(feature);
        cache.Write(key, model);

        return Ok(model);
    }

    [HttpGet("features/resolve")]
    [AllowAnonymous]
    public async Task<ActionResult> Resolve([FromQuery] RepositoryId repository, [FromQuery] string name)
    {
        var graph = await db.ResolveFeature(repository, name);
        var markdown = md.FormatMarkdownDocument(graph.Description, withSyntaxHighlighting: true);
        return Ok(graph with { Description = markdown });
    }

    [HttpGet("features/create")]
    [Authorize("github")]
    public async Task<ActionResult<FeatureEditViewModel>> Create([FromQuery] RepositoryId repository = RepositoryId.Rubberduck, [FromQuery] int? parentId = default)
    {
        var features = await GetFeatureOptions(repository);
        var parent = features.SingleOrDefault(e => e.Id == parentId);

        var model = FeatureEditViewModel.Default(repository, parent, features, RepositoryOptions);
        return Ok(model);
    }

    [HttpPost("create")]
    [Authorize("github")]
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
    [Authorize("github")]
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
    public IActionResult FormatMarkdown([FromBody] MarkdownFormattingRequestViewModel model)
    {
        return Ok(md.FormatMarkdownDocument(model.MarkdownContent, model.WithVbeCodeBlocks));
    }
}
