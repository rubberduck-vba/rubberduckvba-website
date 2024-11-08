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

    public ConcurrentBag<TagGraph> Tags { get; } = [];

    public IEnumerable<Inspection> Inspections { get; set; } = [];
    public IEnumerable<QuickFix> QuickFixes { get; set; } = [];
    public IEnumerable<Annotation> Annotations { get; set; } = [];
}
