using rubberduckvba.Server.Model;
using System.Diagnostics.CodeAnalysis;

namespace rubberduckvba.Server.Data;

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
