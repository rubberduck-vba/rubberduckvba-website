using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;

public interface IXmlDocMerge
{
    IEnumerable<Inspection> Merge(IDictionary<string, Inspection> dbItems, IEnumerable<Inspection> main, IEnumerable<Inspection> next);
    IEnumerable<QuickFix> Merge(IDictionary<string, QuickFix> dbItems, IEnumerable<QuickFix> main, IEnumerable<QuickFix> next);
    IEnumerable<Annotation> Merge(IDictionary<string, Annotation> dbItems, IEnumerable<Annotation> main, IEnumerable<Annotation> next);
}
