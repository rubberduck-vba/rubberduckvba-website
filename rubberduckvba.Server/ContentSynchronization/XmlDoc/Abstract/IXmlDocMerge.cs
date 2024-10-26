using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;

public interface IXmlDocMerge
{
    IEnumerable<FeatureXmlDoc> Merge(IDictionary<string, FeatureXmlDoc> dbItems, IEnumerable<FeatureXmlDoc> main, IEnumerable<FeatureXmlDoc> next);
}
