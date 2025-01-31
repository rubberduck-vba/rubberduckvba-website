using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public class XmlDocMerge(ILogger<XmlDocMerge> logger) : IXmlDocMerge
{
    //public IEnumerable<T> Merge<T>(IDictionary<string, T> dbItems, IEnumerable<T> main, IEnumerable<T> next) where T : IFeature
    //{
    //    var mainBranch = main.ToHashSet();
    //    var nextBranch = (next.Any() ? next : main).ToHashSet();

    //    var merged = new HashSet<T>();
    //    var timestamp = TimeProvider.System.GetUtcNow().DateTime;

    //    var updatedItems = new HashSet<T>(
    //        from item in nextBranch
    //        where dbItems.ContainsKey(item.Name)
    //        let dbItem = dbItems[item.Name]
    //        where dbItem.GetContentHash() != item.GetContentHash()
    //        select item with
    //        {
    //            Id = dbItem.Id,
    //            IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
    //            IsDiscontinued = dbItem.IsDiscontinued || !nextBranch.Any(a => a.Name == item.Name),
    //            DateTimeUpdated = timestamp
    //        }
    //    );

    //    var comparer = new XmlDocBranchIntersectComparer<T>();
    //    var insertItems = new HashSet<T>(
    //        from item in nextBranch.Intersect(mainBranch, comparer)
    //        where !dbItems.ContainsKey(item.Name)
    //        select item with
    //        {
    //            IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
    //            IsDiscontinued = !nextBranch.Any(a => a.Name == item.Name),
    //            DateTimeInserted = timestamp
    //        }
    //    );

    //    return merged;
    //}

    public IEnumerable<Inspection> Merge(IDictionary<string, Inspection> dbItems, IEnumerable<Inspection> main, IEnumerable<Inspection> next)
    {
        var mainBranch = (main.Any() ? main : dbItems.Values).ToHashSet();
        var nextBranch = (next.Any() ? next : mainBranch).ToHashSet();

        var merged = new HashSet<Inspection>();
        var timestamp = TimeProvider.System.GetUtcNow().DateTime;

        var updatedItems = new HashSet<Inspection>(
            from item in nextBranch
            where dbItems.ContainsKey(item.Name)
            let dbItem = dbItems[item.Name]
            where dbItem.GetContentHash() != item.GetContentHash()
            select item with
            {
                Id = dbItem.Id,
                IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                IsDiscontinued = dbItem.IsDiscontinued || !nextBranch.Any(a => a.Name == item.Name),
                DateTimeUpdated = timestamp
            }
        );

        var comparer = new XmlDocBranchIntersectComparer<Inspection>();
        var insertItems = new HashSet<Inspection>(
            (from item in nextBranch.Intersect(mainBranch, comparer)
             where !dbItems.ContainsKey(item.Name)
             select item with
             {
                 IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                 IsDiscontinued = !nextBranch.Any(a => a.Name == item.Name),
                 DateTimeInserted = timestamp
             })
            .Concat(
                from item in nextBranch.Except(mainBranch, comparer)
                where !dbItems.ContainsKey(item.Name)
                select item with
                {
                    IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                    IsDiscontinued = !nextBranch.Any(a => a.Name == item.Name),
                    DateTimeInserted = timestamp
                })
        );

        merged.UnionWith(updatedItems);
        merged.UnionWith(insertItems);
        return merged;
    }


    public IEnumerable<QuickFix> Merge(IDictionary<string, QuickFix> dbItems, IEnumerable<QuickFix> main, IEnumerable<QuickFix> next)
    {
        var mainBranch = (main.Any() ? main : dbItems.Values).ToHashSet();
        var nextBranch = (next.Any() ? next : mainBranch).ToHashSet();

        var merged = new HashSet<QuickFix>();
        var timestamp = TimeProvider.System.GetUtcNow().DateTime;

        var updatedItems = new HashSet<QuickFix>(
            from item in nextBranch
            where dbItems.ContainsKey(item.Name)
            let dbItem = dbItems[item.Name]
            where dbItem.GetContentHash() != item.GetContentHash()
            select item with
            {
                Id = dbItem.Id,
                IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                IsDiscontinued = dbItem.IsDiscontinued || !nextBranch.Any(a => a.Name == item.Name),
                DateTimeUpdated = timestamp
            }
        );

        var comparer = new XmlDocBranchIntersectComparer<QuickFix>();
        var insertItems = new HashSet<QuickFix>(
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

    public IEnumerable<Annotation> Merge(IDictionary<string, Annotation> dbItems, IEnumerable<Annotation> main, IEnumerable<Annotation> next)
    {
        var mainBranch = (main.Any() ? main : dbItems.Values).ToHashSet();
        var nextBranch = (next.Any() ? next : mainBranch).ToHashSet();

        var merged = new HashSet<Annotation>();
        var timestamp = TimeProvider.System.GetUtcNow().DateTime;

        var updatedItems = new HashSet<Annotation>(
            from item in nextBranch
            where dbItems.ContainsKey(item.Name)
            let dbItem = dbItems[item.Name]
            where dbItem.GetContentHash() != item.GetContentHash()
            select item with
            {
                Id = dbItem.Id,
                IsNew = mainBranch.Any() && !mainBranch.Any(a => a.Name == item.Name),
                IsDiscontinued = dbItem.IsDiscontinued || !nextBranch.Any(a => a.Name == item.Name),
                DateTimeUpdated = timestamp
            }
        );

        var comparer = new XmlDocBranchIntersectComparer<Annotation>();
        var insertItems = new HashSet<Annotation>(
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
