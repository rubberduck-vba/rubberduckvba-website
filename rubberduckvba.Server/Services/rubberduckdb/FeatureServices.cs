using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Services.rubberduckdb;

public class FeatureServices(
    IMarkdownFormattingService markdown,
    IRepository<FeatureEntity> featureRepository,
    IRepository<InspectionEntity> inspectionRepository,
    IRepository<QuickFixEntity> quickfixRepository,
    IRepository<AnnotationEntity> annotationRepository)
{
    public int GetId(string name) => featureRepository.GetId(name);
    public IEnumerable<Feature> Get(bool topLevelOnly = true)
    {
        return featureRepository.GetAll()
            .Where(e => !topLevelOnly || e.FeatureId is null)
            .Select(e => new Feature(e));
    }

    public Inspection GetInspection(string name)
    {
        var id = inspectionRepository.GetId(name);
        return new Inspection(inspectionRepository.GetById(id));
    }
    public Annotation GetAnnotation(string name)
    {
        var id = annotationRepository.GetId(name);
        return new Annotation(annotationRepository.GetById(id));
    }
    public QuickFix GetQuickFix(string name)
    {
        var id = quickfixRepository.GetId(name);
        return new QuickFix(quickfixRepository.GetById(id));
    }

    public FeatureGraph Get(string name)
    {
        var id = featureRepository.GetId(name);
        var feature = featureRepository.GetById(id);
        var children = featureRepository.GetAll(parentId: id).Select(e =>
            new Feature(e with
            {
                Description = markdown.FormatMarkdownDocument(e.Description, withSyntaxHighlighting: true),
                ShortDescription = markdown.FormatMarkdownDocument(e.ShortDescription),
            })).ToList();

        IEnumerable<Inspection> inspections = [];
        IEnumerable<QuickFix> quickfixes = [];
        IEnumerable<Annotation> annotations = [];

        if (string.Equals(name, "inspections", StringComparison.InvariantCultureIgnoreCase))
        {
            inspections = inspectionRepository.GetAll()
                .Select(e => new Inspection(e))
                .ToList();
        }
        else if (string.Equals(name, "quickfixes", StringComparison.InvariantCultureIgnoreCase))
        {
            quickfixes = quickfixRepository.GetAll()
                .Select(e => new QuickFix(e))
                .ToList();
        }
        else if (string.Equals(name, "annotations", StringComparison.InvariantCultureIgnoreCase))
        {
            annotations = annotationRepository.GetAll()
                .Select(e => new Annotation(e))
                .ToList();
        }

        return new FeatureGraph(
            feature with
            {
                Description = markdown.FormatMarkdownDocument(feature.Description, withSyntaxHighlighting: true),
                ShortDescription = markdown.FormatMarkdownDocument(feature.ShortDescription),
            })
        {
            Features = children,
            Annotations = annotations,
            QuickFixes = quickfixes,
            Inspections = inspections,
        };
    }

    public void Update(IEnumerable<FeatureGraph> features) => featureRepository.Update(features.Select(feature => feature.ToEntity()));
    public void Update(FeatureGraph feature) => Update([feature]);
    public void Update(IEnumerable<Inspection> inspections) => inspectionRepository.Update(inspections.Select(inspection => inspection.ToEntity()));
    public void Update(IEnumerable<QuickFix> quickFixes) => quickfixRepository.Update(quickFixes.Select(quickfix => quickfix.ToEntity()));
    public void Update(IEnumerable<Annotation> annotations) => annotationRepository.Update(annotations.Select(annotation => annotation.ToEntity()));

    public void Insert(IEnumerable<FeatureGraph> features) => featureRepository.Insert(features.Select(feature => feature.ToEntity()));
    public void Insert(FeatureGraph feature) => Insert([feature]);
    public void Insert(IEnumerable<Inspection> inspections) => inspectionRepository.Insert(inspections.Select(inspection => inspection.ToEntity()));
    public void Insert(IEnumerable<QuickFix> quickFixes) => quickfixRepository.Insert(quickFixes.Select(quickfix => quickfix.ToEntity()));
    public void Insert(IEnumerable<Annotation> annotations) => annotationRepository.Insert(annotations.Select(annotation => annotation.ToEntity()));
}
