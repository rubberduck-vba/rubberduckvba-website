using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc;

public class XmlDocMerge(ILogger<XmlDocMerge> logger) : IXmlDocMerge
{

    public IEnumerable<FeatureXmlDoc> Merge(IDictionary<string, FeatureXmlDoc> dbItems, IEnumerable<FeatureXmlDoc> main, IEnumerable<FeatureXmlDoc> next)
    {
        var mainBranch = main.ToHashSet();
        var nextBranch = (next.Any() ? next : main).ToHashSet();

        var isInitialLoad = !dbItems.Any();

        var comparer = new XmlDocBranchIntersectComparer();
        var timestamp = TimeProvider.System.GetUtcNow().DateTime;

        var merged = new HashSet<FeatureXmlDoc>();

        var updatedItems = new HashSet<FeatureXmlDoc>(
            from item in nextBranch
            where dbItems.ContainsKey(item.Name)
            let dbItem = dbItems[item.Name]
            where dbItem.Serialized != item.Serialized
            select item with
            {
                Id = dbItem.Id,
                IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                IsDiscontinued = dbItem.IsDiscontinued || !nextBranch.Any(a => a.Name == item.Name),
                DateTimeUpdated = timestamp
            }
        );

        var insertItems = new HashSet<FeatureXmlDoc>(
            from item in nextBranch.Intersect(mainBranch, comparer)
            where !dbItems.ContainsKey(item.Name)
            select item with
            {
                IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                IsDiscontinued = !nextBranch.Any(a => a.Name == item.Name),
                DateTimeInserted = timestamp
            }
        );

        merged.UnionWith(updatedItems);
        merged.UnionWith(insertItems);
        return merged;
    }
}
