using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public static class TagExtensions
{
    public static (TagGraph main, TagGraph next, IEnumerable<TagGraph> others) GetLatestTags(this IEnumerable<TagGraph> tags)
    {
        var sortedTags = tags.OrderByDescending(tag => tag.DateCreated)
            .GroupBy(tag => tag.IsPreRelease)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

        return (
            sortedTags[false].First(),
            sortedTags[true].First(),
            sortedTags[false].Skip(1).Concat(sortedTags[true].Skip(1).ToArray())
        );
    }
}