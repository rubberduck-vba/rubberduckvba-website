using System.ComponentModel;

namespace rubberduckvba.com.Server.Data;

public enum ExampleModuleType
{
    None = 0,
    [Description("(Any)")] Any,
    [Description("Class Module")] ClassModule,
    [Description("Document Module")] DocumentModule,
    [Description("Interface Module")] InterfaceModule,
    [Description("Predeclared Class")] PredeclaredClass,
    [Description("Standard Module")] StandardModule,
    [Description("UserForm Module")] UserFormModule
}

public record class ExampleModule
{
    public static ExampleModule ParseError(string name) => new()
    {
        ModuleName = name,
        HtmlContent = "(error parsing code example from source xmldoc)"
    };

    public int ExampleId { get; init; }
    public int SortOrder { get; init; }
    public string ModuleName { get; init; } = default!;
    public ExampleModuleType ModuleType { get; init; }
    public string ModuleTypeName { get; init; }
    public string Properties { get; init; } = default!;
    public string HtmlContent { get; init; } = default!;

    public Example Example { get; init; } = default!;
}

public record class Example
{
    public int FeatureItemId { get; init; }
    public int SortOrder { get; set; }
    public string Properties { get; init; } = default!;

    public FeatureXmlDoc FeatureItem { get; init; } = default!;
    public ICollection<ExampleModule> Modules { get; init; } = [];
}
