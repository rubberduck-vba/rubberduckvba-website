using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Api.Features;

public class FeatureViewModel
{
    public FeatureViewModel(Feature model, bool summaryOnly = false)
    {
        Id = model.Id;
        DateInserted = model.DateTimeInserted;
        DateUpdated = model.DateTimeUpdated;

        FeatureId = model.ParentId;
        FeatureName = model.FeatureName;

        Name = model.Name;
        Title = string.IsNullOrWhiteSpace(model.Title) ? model.Name : model.Title;

        ShortDescription = model.ShortDescription;
        Description = summaryOnly ? string.Empty : model.Description;

        IsNew = model.IsNew;
        IsDiscontinued = model.IsDiscontinued;
        IsHidden = model.IsHidden;

        HasImage = model.HasImage;
        Links = model.Links.ToArray();

        if (!summaryOnly && model is FeatureGraph graph)
        {
            Features = graph.Features.Select(e => new FeatureViewModel(e) { FeatureId = graph.Id, FeatureName = graph.Name }).ToArray();
        }
    }

    public int Id { get; init; }
    public DateTime DateInserted { get; init; }
    public DateTime? DateUpdated { get; init; }

    public int? FeatureId { get; init; }
    public string? FeatureName { get; init; }

    public string Name { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Description { get; init; }
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }
    public bool HasImage { get; init; }

    public FeatureViewModel[] Features { get; init; } = [];
    public BlogLink[] Links { get; init; } = [];
}

public class InspectionViewModel
{
    public InspectionViewModel(Inspection model, IEnumerable<QuickFixViewModel> quickFixes, IDictionary<int, Tag> tagsByAssetId)
    {
        Id = model.Id;
        DateTimeInserted = model.DateTimeInserted;
        DateTimeUpdated = model.DateTimeUpdated;

        FeatureId = model.FeatureId;
        FeatureName = model.FeatureName;

        SourceUrl = model.SourceUrl;
        TagAssetId = model.TagAssetId;
        TagName = tagsByAssetId[model.TagAssetId].Name;

        IsHidden = model.IsHidden;
        IsNew = model.IsNew;
        IsDiscontinued = model.IsDiscontinued;

        Name = model.Name;
        Summary = model.Summary;
        Remarks = model.Remarks;

        InspectionType = model.InspectionType;
        DefaultSeverity = model.DefaultSeverity;
        QuickFixes = quickFixes.Where(e => model.QuickFixes.Any(name => string.Equals(e.Name, name, StringComparison.InvariantCultureIgnoreCase))).ToArray();

        Reasoning = model.Reasoning;
        HostApp = model.HostApp;
        References = model.References;

        Examples = model.Examples;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;

    public int FeatureId { get; init; }
    public string FeatureName { get; init; }

    public string TagName { get; init; }

    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }

    public string Name { get; init; } = string.Empty;
    public string InspectionType { get; init; } = string.Empty;
    public string DefaultSeverity { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public string? HostApp { get; init; }
    public string[] References { get; init; } = [];
    public QuickFixViewModel[] QuickFixes { get; init; } = [];
    public InspectionExample[] Examples { get; init; } = [];
}

public class AnnotationViewModel
{
    public AnnotationViewModel(Annotation model, IDictionary<int, Tag> tagsByAssetId)
    {
        Id = model.Id;
        DateTimeInserted = model.DateTimeInserted;
        DateTimeUpdated = model.DateTimeUpdated;
        FeatureId = model.FeatureId;

        SourceUrl = model.SourceUrl;
        TagAssetId = model.TagAssetId;
        TagName = tagsByAssetId[model.TagAssetId].Name;

        IsHidden = model.IsHidden;
        IsNew = model.IsNew;
        IsDiscontinued = model.IsDiscontinued;

        Name = model.Name;
        Summary = model.Summary;
        Remarks = model.Remarks;

        Parameters = model.Parameters;
        Examples = model.Examples;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public int FeatureId { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;

    public string TagName { get; init; }

    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
    public string? Remarks { get; init; }

    public AnnotationParameter[] Parameters { get; init; } = [];
    public AnnotationExample[] Examples { get; init; } = [];
}

public class QuickFixViewModel
{
    public QuickFixViewModel(QuickFix model, IDictionary<int, Tag> tagsByAssetId, IDictionary<string, Inspection> inspectionsByName)
    {
        Id = model.Id;
        DateTimeInserted = model.DateTimeInserted;
        DateTimeUpdated = model.DateTimeUpdated;
        FeatureId = model.FeatureId;

        SourceUrl = model.SourceUrl;
        TagAssetId = model.TagAssetId;
        TagName = tagsByAssetId[model.TagAssetId].Name;

        IsHidden = model.IsHidden;
        IsNew = model.IsNew;
        IsDiscontinued = model.IsDiscontinued;

        Name = model.Name;
        Summary = model.Summary;
        Remarks = model.Remarks;

        CanFixAll = model.CanFixAll;
        CanFixMultiple = model.CanFixMultiple;
        CanFixProcedure = model.CanFixProcedure;
        CanFixModule = model.CanFixModule;
        CanFixProject = model.CanFixProject;

        Inspections = (from name in model.Inspections
                       let inspection = inspectionsByName[name]
                       select new QuickFixInspectionLinkViewModel
                       {
                           Name = inspection.Name,
                           Summary = inspection.Summary,
                           InspectionType = inspection.InspectionType,
                           DefaultSeverity = inspection.DefaultSeverity
                       }).ToArray();

        Examples = model.Examples;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public int FeatureId { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;

    public string TagName { get; init; }

    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
    public string? Remarks { get; init; }

    public bool CanFixProcedure { get; init; }
    public bool CanFixModule { get; init; }
    public bool CanFixProject { get; init; }
    public bool CanFixAll { get; init; }
    public bool CanFixMultiple { get; init; }

    public QuickFixInspectionLinkViewModel[] Inspections { get; init; } = [];
    public QuickFixExample[] Examples { get; init; } = [];
}

public record class QuickFixInspectionLinkViewModel
{
    public string Name { get; init; }
    public string Summary { get; init; }
    public string InspectionType { get; init; }
    public string DefaultSeverity { get; init; }
}

public class InspectionsFeatureViewModel : FeatureViewModel
{
    public InspectionsFeatureViewModel(FeatureGraph model, IEnumerable<QuickFixViewModel> quickFixes, IDictionary<int, Tag> tagsByAssetId, bool summaryOnly = false)
        : base(model, summaryOnly)
    {

        Inspections = model.Inspections.OrderBy(e => e.Name).Select(e => new InspectionViewModel(e, quickFixes, tagsByAssetId)).ToArray();
    }

    public InspectionViewModel[] Inspections { get; init; } = [];
}

public class QuickFixesFeatureViewModel : FeatureViewModel
{
    public QuickFixesFeatureViewModel(FeatureGraph model, IDictionary<int, Tag> tagsByAssetId, IDictionary<string, Inspection> inspectionsByName, bool summaryOnly = false)
        : base(model, summaryOnly)
    {
        QuickFixes = model.QuickFixes.OrderBy(e => e.Name).Select(e => new QuickFixViewModel(e, tagsByAssetId, inspectionsByName)).ToArray();
    }

    public QuickFixViewModel[] QuickFixes { get; init; } = [];
}

public class AnnotationsFeatureViewModel : FeatureViewModel
{
    public AnnotationsFeatureViewModel(FeatureGraph model, IDictionary<int, Tag> tagsByAssetId, bool summaryOnly = false)
        : base(model, summaryOnly)
    {
        Annotations = model.Annotations.OrderBy(e => e.Name).Select(e => new AnnotationViewModel(e, tagsByAssetId)).ToArray();
    }

    public AnnotationViewModel[] Annotations { get; init; } = [];
}
