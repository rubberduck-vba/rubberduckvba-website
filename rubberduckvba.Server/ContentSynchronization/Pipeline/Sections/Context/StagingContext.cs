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

    public ConcurrentBag<Inspection> Inspections { get; set; } = [];
    public ConcurrentBag<QuickFix> QuickFixes { get; set; } = [];
    public ConcurrentBag<Annotation> Annotations { get; set; } = [];
}
