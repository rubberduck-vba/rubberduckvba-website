using rubberduckvba.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.Server.Model;
using System.Reflection;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public class QuickFixProperties
{
    public string Summary { get; set; }
    public string Remarks { get; set; }
    public bool CanFixInProcedure { get; set; }
    public bool CanFixInModule { get; set; }
    public bool CanFixInProject { get; set; }
    public string[] Inspections { get; set; }
}

public class XmlDocQuickFix : IEquatable<XmlDocQuickFix>
{
    public QuickFix Parse(string name, int assetId, int featureId, XElement node, bool isPreRelease)
    {
        var sourceObject = name;
        var typeName = name/*.Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1)/*.Replace("QuickFix", string.Empty)*/;

        sourceObject = node.Attribute("name")?.Value.Substring(2) ?? string.Empty;

        var summary = node.Element(XmlDocSchema.QuickFix.Summary.ElementName)?.Value.Trim().Replace("  ", " ") ?? string.Empty;
        var remarks = node.Element(XmlDocSchema.QuickFix.Remarks.ElementName)?.Value.Trim().Replace("  ", " ") ?? string.Empty;

        var canFixNode = node.Element(XmlDocSchema.QuickFix.CanFix.ElementName);
        var canFixInProcedure = Convert.ToBoolean(canFixNode?.Attribute(XmlDocSchema.QuickFix.CanFix.ProcedureAttribute)?.Value ?? true.ToString());
        var canFixInModule = Convert.ToBoolean(canFixNode?.Attribute(XmlDocSchema.QuickFix.CanFix.ModuleAttribute)?.Value ?? true.ToString());
        var canFixInProject = Convert.ToBoolean(canFixNode?.Attribute(XmlDocSchema.QuickFix.CanFix.ProjectAttribute)?.Value ?? true.ToString());

        var nodes = node.Element(XmlDocSchema.QuickFix.Inspections.ElementName)?
                        .Elements(XmlDocSchema.QuickFix.Inspections.Inspection.ElementName);

        string[] inspections = [];
        if (nodes is IEnumerable<XElement> quickfixNodes)
        {
            inspections = quickfixNodes
                .Select(e => e.Attribute(XmlDocSchema.QuickFix.Inspections.Inspection.NameAttribute)?.Value.Replace("Inspection", string.Empty))
                .OfType<string>()
                .ToArray();
        }

        return new QuickFix
        {
            FeatureId = featureId,

            Name = name,
            IsHidden = false,
            IsNew = isPreRelease,

            Summary = summary.Trim().Replace("  ", " "),
            TagAssetId = assetId,
            SourceUrl = sourceObject,
            Examples = ParseExamples(node).ToArray(),
        };
    }

    public string SourceObject { get; init; }
    public string TypeName { get; init; }

    public string QuickFixName { get; init; }

    public string Summary { get; init; }
    public string Remarks { get; init; }
    public bool IsPreRelease { get; init; }

    public bool CanFixInProcedure { get; init; }
    public bool CanFixInModule { get; init; }
    public bool CanFixInProject { get; init; }

    public BeforeAndAfterCodeExample[] Examples { get; init; } = [];

    public bool Equals(XmlDocQuickFix? other) => other?.QuickFixName == QuickFixName;

    public override bool Equals(object? obj) => Equals(obj as XmlDocQuickFix);
    public override int GetHashCode() => QuickFixName.GetHashCode();

    private static readonly IDictionary<string, ExampleModuleType> ModuleTypes =
        typeof(ExampleModuleType)
            .GetMembers()
            .Select(m => (m.Name, Description: m.GetCustomAttributes().OfType<System.ComponentModel.DescriptionAttribute>().SingleOrDefault()?.Description ?? string.Empty))
            .Where(m => m.Description != null)
            .ToDictionary(m => m.Description, m => (ExampleModuleType)Enum.Parse(typeof(ExampleModuleType), m.Name, true));

    private IEnumerable<QuickFixExample> ParseExamples(XElement node)
    {
        var results = new List<QuickFixExample>();
        foreach (var exampleNode in node.Elements(XmlDocSchema.QuickFix.Example.ElementName))
        {
            var before = exampleNode.Element(XmlDocSchema.QuickFix.Example.Before.ElementName)?
                .Elements(XmlDocSchema.QuickFix.Example.Before.Module.ElementName)?.OfType<XElement>().Select(m =>
                    new ExampleModule
                    {
                        ModuleName = m.Attribute(XmlDocSchema.QuickFix.Example.Before.Module.ModuleNameAttribute)?.Value ?? string.Empty,
                        ModuleType = ModuleTypes.TryGetValue(m.Attribute(XmlDocSchema.QuickFix.Example.Before.Module.ModuleTypeAttribute)?.Value ?? string.Empty, out var type) ? type : ExampleModuleType.Any,
                        HtmlContent = m.Nodes().OfType<XCData>().Single().Value.Trim().Replace("  ", " ")
                    })
                .Concat(exampleNode.Element(XmlDocSchema.QuickFix.Example.Before.ElementName)?.Nodes().OfType<XCData>().Take(1).Select(x =>
                    new ExampleModule
                    {
                        ModuleName = "Module1",
                        ModuleType = ExampleModuleType.Any,
                        HtmlContent = x.Value.Trim().Replace("  ", " ")
                    }) ?? []);

            var after = exampleNode.Element(XmlDocSchema.QuickFix.Example.After.ElementName)?
                .Elements(XmlDocSchema.QuickFix.Example.After.Module.ElementName)?.Select(m =>
                    new ExampleModule
                    {
                        ModuleName = m.Attribute(XmlDocSchema.QuickFix.Example.After.Module.ModuleNameAttribute)?.Value ?? string.Empty,
                        ModuleType = ModuleTypes.TryGetValue(m.Attribute(XmlDocSchema.QuickFix.Example.After.Module.ModuleTypeAttribute)?.Value ?? string.Empty, out var type) ? type : ExampleModuleType.Any,
                        HtmlContent = m.Nodes().OfType<XCData>().Single().Value.Trim().Replace("  ", " ")
                    })
                .Concat(exampleNode.Element(XmlDocSchema.QuickFix.Example.After.ElementName)?.Nodes().OfType<XCData>().Take(1).Select(x =>
                    new ExampleModule
                    {
                        ModuleName = "Module1",
                        ModuleType = ExampleModuleType.Any,
                        HtmlContent = x.Value.Trim().Replace("  ", " ")
                    }) ?? []);

            if (before != null && after != null)
            {
                results.Add(new QuickFixExample { ModulesBefore = before.ToList(), ModulesAfter = after.ToList() });
            }
            else
            {
                // TODO log a warning, something.
            }
        }
        return results;
    }
}
