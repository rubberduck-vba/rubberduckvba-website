using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using System.Reflection;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public class XmlDocAnnotationParser
{
    public Annotation Parse(int assetId, int featureId, string name, XElement node, bool isPreRelease)
    {
        var sourceObject = node.Attribute("name")!.Value[2..][(name.LastIndexOf('.') + 1)..];
        //var sourceEditUrl = $"https://github.com/rubberduck-vba/Rubberduck/edit/next/{sourceObject}.cs";
        //var sourceViewUrl = $"https://github.com/rubberduck-vba/Rubberduck/blob/{tagName}/{sourceObject}.cs";

        var summary = node.Element(XmlDocSchema.Annotation.Summary.ElementName)?.Value.Trim() ?? string.Empty;
        var remarks = node.Element(XmlDocSchema.Annotation.Remarks.ElementName)?.Value.Trim() ?? string.Empty;

        var isHidden = node.Element(XmlDocSchema.Annotation.Summary.ElementName)?.Attribute(XmlDocSchema.Annotation.Summary.IsHiddenAttribute)?.Value.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase) ?? false;

        var parameters = node.Elements(XmlDocSchema.Annotation.Parameter.ElementName)
            .Select(e => (Name: e.Attribute(XmlDocSchema.Annotation.Parameter.NameAttribute)?.Value ?? string.Empty,
                          Required: e.Attribute(XmlDocSchema.Annotation.Parameter.RequiredAttribute)?.Value ?? string.Empty,
                          Type: e.Attribute(XmlDocSchema.Annotation.Parameter.TypeAttribute)?.Value ?? string.Empty,
                          Description: e.Value))
            .Select(e => new AnnotationParameter { Name = e.Name, Type = e.Type, Description = e.Description, Required = e.Required == true.ToString() })
            .ToArray();

        var examples = ParseExamples(node).ToArray();

        return new Annotation
        {
            FeatureId = featureId,
            TagAssetId = assetId,
            SourceUrl = sourceObject,

            Name = name,
            Summary = summary,
            Remarks = remarks,

            IsNew = isPreRelease,
            IsDiscontinued = default,
            IsHidden = isHidden,

            Parameters = parameters,
            Examples = examples
        };
    }

    private IEnumerable<AnnotationExample> ParseExamples(XElement node)
    {
        try
        {
            var examples = new List<AnnotationExample>();
            var exampleNodes = node.Elements(XmlDocSchema.Annotation.Example.ElementName);
            foreach (var example in exampleNodes)
            {
                /* <example>
                 *   <module><![CDATA[
                 *   'VBA CODE
                 *   ]]>
                 *   </module>
                 * </example>
                */
                var modules = example.Elements(XmlDocSchema.Annotation.Example.Module.ElementName).AsParallel();
                var simpleExamples = modules.Where(m => m.Nodes().OfType<XCData>().Any())
                    .Select((e, i) => new AnnotationExample { Modules = [ExtractCodeModule(e, i)] })
                    .ToArray();
                if (simpleExamples.Length > 0)
                {
                    examples.AddRange(simpleExamples);
                    continue;
                }

                IEnumerable<ExampleModule> before = [];
                IEnumerable<ExampleModule>? after = null;

                if (modules.Any())
                {
                    /* <example>
                     *   <module>
                     *     <before><![CDATA[
                     *   'VBA CODE
                     *   ]]>
                     *     </before>
                     *     <after><![CDATA[
                     *   'VBA CODE
                     *   ]]>
                     *     </after>
                     *   </module>
                     * </example>
                    */
                    before = modules.Select((e, i) => ExtractCodeModule(e.Element(XmlDocSchema.Annotation.Example.Module.Before.ElementName)!, i, "(code pane)"));
                }

                if (example.Elements(XmlDocSchema.Annotation.Example.Before.ElementName).Any())
                {
                    /* <example>
                     *   <before>
                     *     <module><![CDATA[
                     *   'VBA CODE
                     *   ]]>
                     *     </module>
                     *   </before>
                     *   <after>
                     *     <module><![CDATA[
                     *   'VBA CODE
                     *   ]]>
                     *     </module>
                     *   </after>
                     * </example>
                    */
                    before = example.Elements(XmlDocSchema.Annotation.Example.Before.ElementName)
                        .Select((e, i) => ExtractCodeModule(e.Element(XmlDocSchema.Annotation.Example.Before.Module.ElementName)!, i, "(code pane)"));
                }

                if (before.Any() && (after?.Any() ?? false))
                {
                    examples.Add(new AnnotationExample { ModulesBefore = before.ToArray(), ModulesAfter = after.ToArray() });
                }
            }
            return examples;
        }
        catch (Exception)
        {
            var errorExample = new[] { ExampleModule.ParseError("AnnotationExample") };
            return [new AnnotationExample { Modules = errorExample }];
        }
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
