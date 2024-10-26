using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using RubberduckServices;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Schema;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc;

public record class InspectionProperties
{
    public string Reasoning { get; init; } = default!;
    public string[] References { get; init; } = [];
    public string[] QuickFixes { get; init; } = [];
    public string HostApp { get; init; } = default!;
    public string Summary { get; init; } = default!;
    public string Remarks { get; init; } = default!;
    public string DefaultSeverity { get; init; } = default!;
    public string InspectionType { get; init; } = default!;
    public Example[] Examples { get; init; } = [];
}

public class XmlDocInspection
{
    private static readonly string _defaultSeverity = "Warning";
    private static readonly string _defaultInspectionType = "CodeQualityIssues";
    private readonly IMarkdownFormattingService _markdownService;

    private InspectionProperties _properties;
    public XmlDocInspection(IMarkdownFormattingService markdownService)
    {
        _markdownService = markdownService;
    }

    public string Properties { get; private set; }

    public string SourceObject { get; }
    public bool IsPreRelease { get; }

    public bool IsHidden { get; }
    public string TypeName { get; }
    public string InspectionName { get; }
    public string Summary { get; }
    public string[] References { get; }
    public string HostApp { get; }
    public string Reasoning { get; }
    public string Remarks { get; }
    public string InspectionType { get; }
    public string DefaultSeverity { get; }

    public Example[] Examples { get; }

    public async Task<FeatureXmlDoc> ParseAsync(int assetId, string tagName, int featureId, IEnumerable<FeatureXmlDoc> quickFixes, string name, XElement node, InspectionDefaultConfig? config, bool isPreRelease)
    {
        var typeName = name.Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1);
        var inspectionName = typeName.Replace("Inspection", string.Empty).Trim();

        var fixes = quickFixes.Select(e => (e.Name, e.Serialized));
        var fixesByName = fixes.ToLookup(e => e.Name, e => JsonSerializer.Deserialize<XmlDocQuickFixInfo>(e.Serialized, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Inspections);
        var filteredFixes = quickFixes.Where(fix => fixesByName[fix.Name].FirstOrDefault()?.Contains(inspectionName) ?? false).ToList();

        var sourceObject = name.Substring(2).Replace(".", "/").Replace("Rubberduck/CodeAnalysis/", "Rubberduck.CodeAnalysis/");
        var sourceEditUrl = $"https://github.com/rubberduck-vba/Rubberduck/edit/next/{sourceObject}.cs";
        var sourceViewUrl = $"https://github.com/rubberduck-vba/Rubberduck/blob/{tagName}/{sourceObject}.cs";

        var summaryTask = _markdownService.FormatMarkdownDocument(node.Element(XmlDocSchema.Inspection.Summary.ElementName)?.Value.Trim() ?? string.Empty);
        var reasoningTask = _markdownService.FormatMarkdownDocument(node.Element(XmlDocSchema.Inspection.Reasoning.ElementName)?.Value.Trim() ?? string.Empty);
        var remarksTask = _markdownService.FormatMarkdownDocument(node.Element(XmlDocSchema.Inspection.Remarks.ElementName)?.Value.Trim() ?? string.Empty);

        var references = node.Elements(XmlDocSchema.Inspection.Reference.ElementName).Select(e => e.Attribute(XmlDocSchema.Inspection.Reference.NameAttribute)!.Value.Trim()).ToArray();
        var hostApp = node.Element(XmlDocSchema.Inspection.HostApp.ElementName)?.Attribute(XmlDocSchema.Inspection.HostApp.NameAttribute)?.Value.Trim() ?? string.Empty;
        var isHidden = node.Element(XmlDocSchema.Inspection.Summary.ElementName)?.Attribute(XmlDocSchema.Inspection.Summary.IsHiddenAttribute)?.Value.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase) ?? false;

        var defaultSeverity = config?.DefaultSeverity ?? _defaultSeverity;
        var inspectionType = config?.InspectionType ?? _defaultInspectionType;

        if (Enum.TryParse<CodeInspectionType>(InspectionType, out var enumInspectionType))
        {
            inspectionType = InspectionTypes[enumInspectionType];
        }
        var examples = ParseExamples(node).ToArray();

        var (summary, reasoning, remarks) = (await summaryTask, await reasoningTask, await remarksTask);

        var properties = new InspectionProperties
        {
            Reasoning = reasoning,
            Summary = summary,
            Remarks = remarks,
            HostApp = hostApp,
            DefaultSeverity = defaultSeverity,
            InspectionType = inspectionType,
            QuickFixes = filteredFixes.Select(e => e.Name).ToArray(),
            References = references,
            Examples = examples
        };

        return new FeatureXmlDoc
        {
            FeatureId = featureId,
            FeatureName = "Inspections",
            FeatureTitle = "Inspections",

            Id = default,
            DateTimeInserted = DateTime.UtcNow,
            DateTimeUpdated = default,
            TagAssetId = assetId,
            SourceUrl = sourceObject,

            Name = inspectionName,
            Title = inspectionName,
            Summary = summary,

            IsHidden = isHidden,
            IsNew = isPreRelease,
            IsDiscontinued = default,

            Examples = examples,
            Serialized = JsonSerializer.Serialize(properties)
        };
    }

    private static readonly IDictionary<string, ExampleModuleType> ModuleTypes =
        Enum.GetValues<ExampleModuleType>()
            .Select(m => (Name: m.ToString(), Description: typeof(ExampleModuleType).GetMember(m.ToString()).Single()
                .GetCustomAttributes().OfType<DescriptionAttribute>().SingleOrDefault()?.Description ?? string.Empty))
            .ToDictionary(m => m.Description, m => Enum.Parse<ExampleModuleType>(m.Name, ignoreCase: true));

    private static readonly IDictionary<CodeInspectionType, string> InspectionTypes =
        Enum.GetValues<CodeInspectionType>()
            .Select(m => (Name: m.ToString(), Description: typeof(CodeInspectionType).GetMember(m.ToString()).Single()
                .GetCustomAttributes().OfType<DescriptionAttribute>().SingleOrDefault()?.Description ?? string.Empty))
            .ToDictionary(m => Enum.Parse<CodeInspectionType>(m.Name, ignoreCase: true), m => m.Description);

    private enum CodeInspectionType
    {
        [Display(Name = "Rubberduck Opportunities")]
        RubberduckOpportunities,
        [Display(Name = "Language Opportunities")]
        LanguageOpportunities,
        [Display(Name = "Maintainability/Readability Issues")]
        MaintainabilityAndReadabilityIssues,
        [Display(Name = "Code Quality Issues")]
        CodeQualityIssues,
        [Display(Name = "Performance Opportunities")]
        Performance,
    }

    private IEnumerable<Example> ParseExamples(XElement node)
    {
        return node.Elements(XmlDocSchema.Inspection.Example.ElementName).AsParallel()
            .Select((e, i) =>
                {
                    var hasResult = e.Attributes().Any(a => string.Equals(a.Name.LocalName, XmlDocSchema.Inspection.Example.HasResultAttribute, StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(a.Value, out var value) && value);
                    var example = new Example
                    {
                        Properties = $"{{\"HasResult\":\"{hasResult}\"}}",
                        SortOrder = i,
                        Modules = e.Elements(XmlDocSchema.Inspection.Example.Module.ElementName)
                        .Select(async m =>
                            new ExampleModule
                            {
                                HtmlContent = await _markdownService.FormatMarkdownDocument("<code>" + m.Nodes().OfType<XCData>().Single().Value.Trim().Replace("  ", " ") + "</code>", withSyntaxHighlighting: true),
                                ModuleName = m.Attribute(XmlDocSchema.Inspection.Example.Module.ModuleNameAttribute)?.Value ?? string.Empty,
                                ModuleType = ModuleTypes.TryGetValue(m.Attribute(XmlDocSchema.Inspection.Example.Module.ModuleTypeAttribute)?.Value ?? string.Empty, out var type) ? type : ExampleModuleType.Any,
                            })
                        .Select(t => t.GetAwaiter().GetResult())
                        .Concat(e.Nodes().OfType<XCData>().Select(async x =>
                            new ExampleModule
                            {
                                HtmlContent = await _markdownService.FormatMarkdownDocument(x.Value, withSyntaxHighlighting: true),
                                ModuleName = "Module1",
                                ModuleType = ExampleModuleType.Any
                            })
                        .Select(t => t.GetAwaiter().GetResult())
                        .Take(1)).ToList()
                    };
                    return example;
                });
    }
}
