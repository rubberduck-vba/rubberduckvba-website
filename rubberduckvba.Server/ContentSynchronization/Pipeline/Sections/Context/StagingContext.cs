using rubberduckvba.Server.Model;
using System.Collections.Concurrent;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;

public class StagingContext
{
    public SyncRequestParameters Parameters { get; }

    public StagingContext(SyncRequestParameters parameters)
    {
        Parameters = parameters;
    }

    public ConcurrentBag<TagGraph> ProcessedTags { get; } = [];
    public ICollection<TagGraph> NewTags { get; set; } = [];
    public ICollection<Tag> UpdatedTags { get; set; } = [];

    public ConcurrentBag<object> ProcessedFeatureItems { get; } = [];
    public ICollection<object> NewFeatureItems { get; set; } = [];
    public ICollection<object> UpdatedFeatureItems { get; set; } = [];


    public IEnumerable<Inspection> NewInspections { get; set; } = [];
    public IEnumerable<Inspection> UpdatedInspections { get; set; } = [];

    public IEnumerable<QuickFix> NewQuickFixes { get; set; } = [];
    public IEnumerable<QuickFix> UpdatedQuickFixes { get; set; } = [];

    public IEnumerable<Annotation> NewAnnotations { get; set; } = [];
    public IEnumerable<Annotation> UpdatedAnnotations { get; set; } = [];
}
