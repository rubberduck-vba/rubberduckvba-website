using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Services.rubberduckdb;

public class FeatureServices(
    IRepository<FeatureEntity> featureRepository,
    IRepository<InspectionEntity> inspectionRepository,
    IRepository<QuickFixEntity> quickfixRepository,
    IRepository<AnnotationEntity> annotationRepository)
{
    public int GetId(string name) => featureRepository.GetId(name);
    public IEnumerable<Feature> Get(bool topLevelOnly = true)
    {
        return featureRepository.GetAll()
            .Where(e => !topLevelOnly || e.ParentId is null)
            .Select(e => new Feature(e));
    }

    public FeatureGraph Get(string name)
    {
        var id = featureRepository.GetId(name);
        var feature = featureRepository.GetById(id);

        IEnumerable<Inspection> inspections = [];
        IEnumerable<QuickFix> quickfixes = [];
        IEnumerable<Annotation> annotations = [];

        if (string.Equals(name, "inspections", StringComparison.InvariantCultureIgnoreCase))
        {
            inspections = inspectionRepository.GetAll().Select(e => new Inspection(e)).ToList();
        }
        else if (string.Equals(name, "quickfixes", StringComparison.InvariantCultureIgnoreCase))
        {
            quickfixes = quickfixRepository.GetAll().Select(e => new QuickFix(e)).ToList();
        }
        else if (string.Equals(name, "annotations", StringComparison.InvariantCultureIgnoreCase))
        {
            annotations = annotationRepository.GetAll().Select(e => new Annotation(e)).ToList();
        }

        return new FeatureGraph(feature)
        {
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
