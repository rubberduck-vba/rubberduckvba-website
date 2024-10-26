using rubberduckvba.com.Server.Data;
using System.Collections.Concurrent;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline;

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

    public ConcurrentBag<FeatureXmlDoc> ProcessedFeatureItems { get; } = [];
    public ICollection<FeatureXmlDoc> NewFeatureItems { get; set; } = [];
    public ICollection<FeatureXmlDoc> UpdatedFeatureItems { get; set; } = [];
}
