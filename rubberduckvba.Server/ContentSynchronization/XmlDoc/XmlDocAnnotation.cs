using Newtonsoft.Json;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using System.Reflection;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public class AnnotationProperties
{
    public AnnotationArgInfo[] Parameters { get; set; } = [];
}

public class XmlDocAnnotation
{
    public XmlDocAnnotation(string name, XElement node, bool isPreRelease)
    {
        SourceObject = node.Attribute("name")!.Value.Substring(2).Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1);
        IsPreRelease = isPreRelease;

        AnnotationName = name;
        Summary = node.Element(XmlDocSchema.Annotation.Summary.ElementName)?.Value.Trim() ?? string.Empty;
        Remarks = node.Element(XmlDocSchema.Annotation.Remarks.ElementName)?.Value.Trim() ?? string.Empty;

        Parameters = node.Elements(XmlDocSchema.Annotation.Parameter.ElementName)
            .Select(e => (Name: e.Attribute(XmlDocSchema.Annotation.Parameter.NameAttribute)?.Value ?? string.Empty,
                          Type: e.Attribute(XmlDocSchema.Annotation.Parameter.TypeAttribute)?.Value ?? string.Empty,
                          Description: e.Value))
            .Select(e => new AnnotationArgInfo(e.Name, e.Type, e.Description))
            .ToArray();

        Properties = JsonConvert.SerializeObject(Parameters);

        Examples = [];
    }

    public string SourceObject { get; }
    public string Properties { get; }
    public bool IsPreRelease { get; }

    public string AnnotationName { get; }
    public string Summary { get; }
    public string Remarks { get; }

    public IReadOnlyList<AnnotationArgInfo> Parameters { get; }
    public IReadOnlyList<BeforeAndAfterCodeExample> Examples { get; }

    public Annotation Parse(int assetId, int featureId)
    {
        var parameters = new AnnotationProperties
        {
            Parameters = Parameters.ToArray()
        };

        return new Annotation
        {
            FeatureId = featureId,
            Name = AnnotationName,
            IsNew = IsPreRelease,
            TagAssetId = assetId,
            SourceUrl = SourceObject,

            Examples = []
        };
    }

    private AnnotationExample[] ParseExamples(XElement node)
    {
        //try
        //{
        //    var examples = new List<BeforeAndAfterCodeExample>();
        //    var exampleNodes = node.Elements(XmlDocSchema.Annotation.Example.ElementName);
        //    foreach (var example in exampleNodes)
        //    {
        //        /* <example>
        //         *   <module><![CDATA[
        //         *   'VBA CODE
        //         *   ]]>
        //         *   </module>
        //         * </example>
        //        */
        //        var modules = example.Elements(XmlDocSchema.Annotation.Example.Module.ElementName).AsParallel();
        //        var simpleExamples = modules.Where(m => m.Nodes().OfType<XCData>().Any())
        //            .Select((e, i) => new AnnotationExample { Modules = [ExtractCodeModule(e, i)] })
        //            .ToArray();
        //        if (simpleExamples.Length > 0)
        //        {
        //            examples.AddRange(simpleExamples.ToArray());
        //            continue;
        //        }

        //        IEnumerable<ExampleModule> before = Enumerable.Empty<ExampleModule>();
        //        IEnumerable<ExampleModule>? after = null;

        //        if (modules.Any())
        //        {
        //            /* <example>
        //             *   <module>
        //             *     <before><![CDATA[
        //             *   'VBA CODE
        //             *   ]]>
        //             *     </before>
        //             *     <after><![CDATA[
        //             *   'VBA CODE
        //             *   ]]>
        //             *     </after>
        //             *   </module>
        //             * </example>
        //            */
        //            before = modules.Select((e, i) => ExtractCodeModule(e.Element(XmlDocSchema.Annotation.Example.Module.Before.ElementName)!, i, "(code pane)"));
        //        }

        //        if (example.Elements(XmlDocSchema.Annotation.Example.Before.ElementName).Any())
        //        {
        //            /* <example>
        //             *   <before>
        //             *     <module><![CDATA[
        //             *   'VBA CODE
        //             *   ]]>
        //             *     </module>
        //             *   </before>
        //             *   <after>
        //             *     <module><![CDATA[
        //             *   'VBA CODE
        //             *   ]]>
        //             *     </module>
        //             *   </after>
        //             * </example>
        //            */
        //            before = example.Elements(XmlDocSchema.Annotation.Example.Before.ElementName)
        //                .Select((e, i) => ExtractCodeModule(e.Element(XmlDocSchema.Annotation.Example.Before.Module.ElementName)!, i, "(code pane)"));
        //        }

        //        if (before.Any() && after.Any())
        //        {
        //            examples.Add(new BeforeAndAfterCodeExample(before, after));
        //        }
        //    }
        return [];
        //}
        //catch (Exception)
        //{
        //    var errorExample = new[] { ExampleModule.ParseError("AnnotationExample") };
        //    return [new AnnotationExample { Modules = errorExample }];
        //}
    }

    private static readonly IDictionary<string, ExampleModuleType> ModuleTypes =
        typeof(ExampleModuleType)
            .GetMembers()
            .Select(m => (m.Name, m.GetCustomAttributes().OfType<System.ComponentModel.DescriptionAttribute>().SingleOrDefault()?.Description))
            .Where(m => m.Description != null)
            .ToDictionary(m => m.Description!, m => (ExampleModuleType)Enum.Parse(typeof(ExampleModuleType), m.Name, true));

    private static string GetDefaultModuleName(ExampleModuleType type, int index = 1)
    {
        switch (type)
        {
            case ExampleModuleType.PredeclaredClass:
            case ExampleModuleType.ClassModule:
                return $"Class{index}";
            case ExampleModuleType.UserFormModule:
                return $"UserForm{index}";
            case ExampleModuleType.DocumentModule:
                return $"DocumentModule{index}";
            default:
                return $"Module{index}";
        }
    }

    private ExampleModule ExtractCodeModule(XElement cdataParent, int index, string? description = null)
    {
        var module = cdataParent.AncestorsAndSelf(XmlDocSchema.Annotation.Example.Module.ElementName).Single();
        var moduleType = ModuleTypes.TryGetValue(module.Attribute(XmlDocSchema.Annotation.Example.Module.ModuleTypeAttribute)?.Value ?? string.Empty, out var type) ? type : ExampleModuleType.Any;
        var name = module.Attribute(XmlDocSchema.Annotation.Example.Module.ModuleNameAttribute)?.Value ?? GetDefaultModuleName(moduleType, index);
        var code = cdataParent.Nodes().OfType<XCData>().Single().Value;

        var model = new ExampleModule
        {
            ModuleName = name,
            ModuleType = moduleType,
            Description = description,
            HtmlContent = code.Trim().Replace("  ", " ")
        };

        return model;
    }
}

public class AnnotationArgInfo(string name, string type, string description)
{
    public string Name { get; set; } = name;
    public string Type { get; set; } = type;
    public string Description { get; set; } = description;
}
