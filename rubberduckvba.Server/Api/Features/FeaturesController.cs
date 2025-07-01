using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;
using System.Security.Principal;

namespace rubberduckvba.Server.Api.Features;

public class MarkdownContentViewModel
{
    public string Content { get; init; } = string.Empty;
}


[AllowAnonymous]
[EnableCors(CorsPolicies.AllowAll)]
public class FeaturesController : RubberduckApiController
{
    private readonly CacheService cache;
    private readonly IRubberduckDbService db;
    private readonly IAuditService admin;
    private readonly FeatureServices features;
    private readonly IRepository<TagAssetEntity> assetsRepository;
    private readonly IRepository<TagEntity> tagsRepository;
    private readonly IMarkdownFormattingService markdownService;

    public FeaturesController(CacheService cache, IRubberduckDbService db, IAuditService admin, FeatureServices features, IMarkdownFormattingService markdownService,
        IRepository<TagAssetEntity> assetsRepository, IRepository<TagEntity> tagsRepository, ILogger<FeaturesController> logger)
        : base(logger)
    {
        this.cache = cache;
        this.db = db;
        this.admin = admin;
        this.features = features;
        this.assetsRepository = assetsRepository;
        this.tagsRepository = tagsRepository;
        this.markdownService = markdownService;
    }

    private static RepositoryOptionViewModel[] RepositoryOptions { get; } =
        Enum.GetValues<RepositoryId>().Select(e => new RepositoryOptionViewModel { Id = e, Name = e.ToString() }).ToArray();

    private async Task<FeatureOptionViewModel[]> GetFeatureOptions(RepositoryId repositoryId) =>
        await db.GetTopLevelFeatures(repositoryId)
            .ContinueWith(t => t.Result.Select(e => new FeatureOptionViewModel { Id = e.Id, Name = e.Name, Title = e.Title }).ToArray());

    [HttpGet("features")]
    public IActionResult Index()
    {
        return GuardInternalAction(() =>
        {
            FeatureViewModel[]? result = [];
            if (!cache.TryGetFeatures(out result))
            {
                var features = db.GetTopLevelFeatures(RepositoryId.Rubberduck).GetAwaiter().GetResult();
                if (!features.Any())
                {
                    return NoContent();
                }

                result = features
                    .Select(e => new FeatureViewModel(e, summaryOnly: true))
                    .ToArray();

                if (result.Length > 0)
                {
                    cache.Invalidate(result);
                }
            }

            return result is not null && result.Length != 0 ? Ok(result) : NoContent();
        });
    }

    [HttpGet("features/{name}")]
    public IActionResult Info([FromRoute] string name)
    {
        return GuardInternalAction(() =>
        {
            return name.ToLowerInvariant() switch
            {
                "inspections" => Ok(GetInspections()),
                "quickfixes" => Ok(GetQuickFixes()),
                "annotations" => Ok(GetAnnotations()),
                _ => Ok(GetFeature(name))
            };
        });
    }

    [HttpGet("inspections/{name}")]
    public IActionResult Inspection([FromRoute] string name)
    {
        return GuardInternalAction(() =>
        {
            InspectionViewModel? result;
            if (!cache.TryGetInspection(name, out result))
            {
                _ = GetInspections(); // caches all inspections
            }

            if (!cache.TryGetInspection(name, out result))
            {
                return NotFound();
            }

            return Ok(result);
        });
    }

    [HttpGet("annotations/{name}")]
    public IActionResult Annotation([FromRoute] string name)
    {
        return GuardInternalAction(() =>
        {
            AnnotationViewModel? result;
            if (!cache.TryGetAnnotation(name, out result))
            {
                _ = GetAnnotations(); // caches all annotations
            }

            if (!cache.TryGetAnnotation(name, out result))
            {
                return NotFound();
            }

            return Ok(result);
        });
    }

    [HttpGet("quickfixes/{name}")]
    public IActionResult QuickFix([FromRoute] string name)
    {
        return GuardInternalAction(() =>
        {
            QuickFixViewModel? result;
            if (!cache.TryGetQuickFix(name, out result))
            {
                _ = GetAnnotations(); // caches all quickfixes
            }

            if (!cache.TryGetQuickFix(name, out result))
            {
                return NotFound();
            }

            return Ok(result);
        });
    }

    [HttpGet("features/create")]
    [Authorize("github", Roles = $"{RDConstants.Roles.AdminRole},{RDConstants.Roles.ReviewerRole}")]
    public async Task<ActionResult<FeatureEditViewModel>> Create([FromQuery] RepositoryId repository = RepositoryId.Rubberduck, [FromQuery] int? parentId = default)
    {
        var features = await GetFeatureOptions(repository);
        var parent = features.SingleOrDefault(e => e.Id == parentId);

        var model = FeatureEditViewModel.Default(repository, parent, features, RepositoryOptions);
        return Ok(model);
    }

    [HttpPost("features/create")]
    [Authorize("github", Roles = $"{RDConstants.Roles.AdminRole},{RDConstants.Roles.ReviewerRole}")]
    public async Task<ActionResult<FeatureEditViewModel>> Create([FromBody] FeatureEditViewModel model)
    {
        if (model.Id.HasValue || string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length < 3)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var existingId = await db.GetFeatureId(model.RepositoryId, model.Name);
        if (existingId != null)
        {
            return BadRequest($"Model [Name] must be unique; feature '{model.Name}' already exists.");
        }

        var feature = model.ToFeature();
        if (User.Identity is IIdentity identity)
        {
            await admin.CreateFeature(feature, identity);
            return Ok(feature);
        }
        else
        {
            return Unauthorized("User identity is not available.");
        }
    }

    [HttpPost("features/update")]
    [Authorize("github", Roles = $"{RDConstants.Roles.AdminRole},{RDConstants.Roles.ReviewerRole}")]
    public async Task<ActionResult<FeatureEditViewModel>> Update([FromBody] FeatureEditViewModel model)
    {
        if (model.Id.GetValueOrDefault() == default)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var existingId = await db.GetFeatureId(model.RepositoryId, model.Name);
        if (existingId is null)
        {
            return BadRequest("Model is invalid for this endpoint.");
        }

        var feature = model.ToFeature();
        if (User.Identity is IIdentity identity)
        {
            await admin.UpdateFeature(feature, identity);
            return Ok(feature);
        }
        else
        {
            return Unauthorized("User identity is not available.");
        }
    }

    [HttpPost("features/delete")]
    [Authorize("github", Roles = $"{RDConstants.Roles.AdminRole},{RDConstants.Roles.ReviewerRole}")]
    public async Task Delete([FromBody] Feature model)
    {
        if (model.Id == default)
        {
            throw new ArgumentException("Model is invalid for this endpoint.");
        }
        var existing = await db.ResolveFeature(RepositoryId.Rubberduck, model.Name);
        if (existing is null)
        {
            throw new ArgumentException("Model is invalid for this endpoint.");
        }

        if (User.Identity is IIdentity identity)
        {
            await admin.DeleteFeature(existing, identity);
        }
        else
        {
            throw new UnauthorizedAccessException("User identity is not available.");
        }
    }

    [HttpPost("markdown/format")]
    public MarkdownContentViewModel FormatMarkdown([FromBody] MarkdownContentViewModel model)
    {
        var markdown = model.Content;
        if (!cache.TryGetHtml(markdown, out var html))
        {
            html = markdownService.FormatMarkdownDocument(markdown, withSyntaxHighlighting: true);
            cache.CacheHtml(markdown, html);
        }

        return new MarkdownContentViewModel { Content = html! };
    }

    private InspectionsFeatureViewModel GetInspections()
    {
        InspectionsFeatureViewModel result;
        if (!cache.TryGetInspections(out result!))
        {
            var quickfixesModel = GetQuickFixes();

            var feature = features.Get("Inspections") as FeatureGraph;
            result = new InspectionsFeatureViewModel(feature, quickfixesModel.QuickFixes,
                feature.Inspections
                    .Select(e => e.TagAssetId).Distinct()
                    .ToDictionary(id => id, id => new Tag(tagsRepository.GetById(assetsRepository.GetById(id).TagId))));

            cache.Invalidate(result);
        }

        return result;
    }

    private QuickFixesFeatureViewModel GetQuickFixes()
    {
        QuickFixesFeatureViewModel result;
        if (!cache.TryGetQuickFixes(out result!))
        {
            var feature = features.Get("QuickFixes") as FeatureGraph;
            result = new QuickFixesFeatureViewModel(feature,
                feature.QuickFixes
                    .Select(e => e.TagAssetId).Distinct()
                    .ToDictionary(id => id, id => new Tag(tagsRepository.GetById(assetsRepository.GetById(id).TagId))),
                features.Get("Inspections").Inspections.ToDictionary(inspection => inspection.Name));

            cache.Invalidate(result);
        }

        return result;
    }

    private AnnotationsFeatureViewModel GetAnnotations()
    {
        AnnotationsFeatureViewModel result;
        if (!cache.TryGetAnnotations(out result!))
        {
            var feature = features.Get("Annotations") as FeatureGraph;
            result = new AnnotationsFeatureViewModel(feature,
                feature.Annotations
                    .Select(e => e.TagAssetId).Distinct()
                    .ToDictionary(id => id, id => new Tag(tagsRepository.GetById(assetsRepository.GetById(id).TagId))));

            cache.Invalidate(result);
        }

        return result;
    }

    private FeatureViewModel GetFeature(string name)
    {
        FeatureViewModel result;
        if (!cache.TryGetFeature(name, out result!))
        {
            var feature = features.Get(name);
            result = new FeatureViewModel(feature);

            cache.Invalidate(result);
        }

        return result;
    }
}