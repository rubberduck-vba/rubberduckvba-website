using rubberduckvba.Server.Model;
using System.Diagnostics.CodeAnalysis;

namespace rubberduckvba.Server.Data;

//public record class Tag
//{
//    public int Id { get; init; }
//    public DateTime DateTimeInserted { get; init; }
//    public DateTime? DateTimeUpdated { get; init; }
//    public long ReleaseId { get; init; }
//    public string Name { get; init; } = default!;
//    public DateTime DateCreated { get; init; }
//    public string InstallerDownloadUrl { get; init; } = default!;
//    public int InstallerDownloads { get; init; }
//    public bool IsPreRelease { get; init; }
//}

//public record class TagAsset
//{
//    public int Id { get; init; }
//    public DateTime DateTimeInserted { get; init; }
//    public DateTime? DateTimeUpdated { get; init; }
//    public int TagId { get; init; }
//    public string Name { get; init; } = default!;
//    public string DownloadUrl { get; init; } = default!;
//}


//public record class TagGraph : Tag
//{
//    public IEnumerable<TagAsset> Assets { get; init; } = [];
//}

//public record class Feature
//{
//    public int Id { get; init; }
//    public int? ParentId { get; init; }
//    public DateTime DateTimeInserted { get; init; }
//    public DateTime? DateTimeUpdated { get; init; }
//    public int RepositoryId { get; init; }
//    public string Name { get; init; } = default!;
//    public string Title { get; init; } = default!;
//    public string ShortDescription { get; init; } = default!;
//    public string Description { get; init; } = default!;
//    public bool IsHidden { get; init; }
//    public bool IsNew { get; init; }
//    public bool HasImage { get; init; }
//}

//public record class FeatureGraph : Feature
//{
//    public static FeatureGraph FromFeature(Feature feature) => new()
//    {
//        Id = feature.Id,
//        DateTimeInserted = feature.DateTimeInserted,
//        DateTimeUpdated = feature.DateTimeUpdated,
//        RepositoryId = feature.RepositoryId,
//        ParentId = feature.ParentId,
//        Name = feature.Name,
//        Title = feature.Title,
//        ShortDescription = feature.ShortDescription,
//        Description = feature.Description,
//        IsHidden = feature.IsHidden,
//        IsNew = feature.IsNew,
//    };

//    public string ParentName { get; init; } = default!;
//    public string ParentTitle { get; init; } = default!;

//    public IEnumerable<Feature> Features { get; init; } = [];
//    public IEnumerable<FeatureXmlDoc> Items { get; init; } = [];

//    public IEnumerable<XmlDocInspectionInfo> Inspections { get; init; } = [];
//}

public record class FeatureXmlDoc<TInfo> : FeatureXmlDoc
{
    public TInfo? Info { get; init; } = default!;
}

public record class FeatureXmlDoc
{
    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }

    public int FeatureId { get; init; }
    public string FeatureName { get; init; }
    public string FeatureTitle { get; init; }

    public string Name { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Summary { get; init; } = default!;

    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }

    public int TagAssetId { get; init; }
    public int TagId { get; init; }

    public string SourceUrl { get; init; } = default!;

    public string Serialized { get; init; } = default!;
    public string TagName { get; init; } = default!;

    public IEnumerable<Example> Examples { get; init; } = [];
}

public class XmlDocBranchIntersectComparer<T> : EqualityComparer<T> where T : IFeature
{
    public override bool Equals(T? x, T? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.Name == y.Name;
    }

    public override int GetHashCode([DisallowNull] T obj)
    {
        return HashCode.Combine(obj.Name);
    }
}

public record class XmlDocInspectionInfo : FeatureXmlDoc<XmlDocInspectionInfo>
{
    public string Reasoning { get; init; } = default!;
    public string Remarks { get; init; } = default!;
    public string DefaultSeverity { get; init; } = default!;
    public string InspectionType { get; init; } = default!;

    public string[] References { get; init; } = [];
    public string[] QuickFixes { get; init; } = [];
    public string? HostApp { get; init; } = default!;


}

public record class XmlDocQuickFixInfo : FeatureXmlDoc<XmlDocQuickFixInfo>
{
    public string Remarks { get; init; } = default!;
    public bool CanFixInProcedure { get; init; }
    public bool CanFixInModule { get; init; }
    public bool CanFixInProject { get; init; }
    public string[] Inspections { get; init; } = [];
}

public record class XmlDocAnnotationInfo : FeatureXmlDoc<XmlDocQuickFixInfo>
{
    public XmlDocAnnotationParameterInfo[] Parameters { get; init; } = [];
}

public record class XmlDocAnnotationParameterInfo
{
    public string Name { get; init; } = default!;
    public string Type { get; init; } = default!;
    public string Description { get; init; } = default!;
}

public enum SyncStatus
{
    Received = 0,
    Started = 1,
    Success = 2,
    Error = -1
}

public record class SynchronizationRequest
{
    public Guid RequestId { get; init; } = default!;
    public string JobId { get; init; } = default!;
    public DateTime UtcDateTimeStarted { get; init; } = default!;
    public DateTime? UtcDateTimeEnded { get; init; } = default!;
    public SyncStatus Status { get; init; } = default!;
    public string Message { get; init; } = default!;
}

public static class SynchronizationMessage
{
    public static string FromStatus(SyncStatus status) => status switch
    {
        SyncStatus.Received => "Synchronization queued.",
        SyncStatus.Started => "Synchronization started.",
        SyncStatus.Success => "Synchronization completed.",
        SyncStatus.Error => "Synchronization failed.",
        _ => "(unknown status)"
    };
}
