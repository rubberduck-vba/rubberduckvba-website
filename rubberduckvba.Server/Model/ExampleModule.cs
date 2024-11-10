using System.Reflection;

namespace rubberduckvba.Server.Model;

public record class ExampleModule
{
    private static readonly IDictionary<ExampleModuleType, string> ModuleTypes =
        typeof(ExampleModuleType)
            .GetMembers()
            .Select(m => (m.Name, m.GetCustomAttributes().OfType<System.ComponentModel.DescriptionAttribute>().SingleOrDefault()?.Description))
            .Where(m => m.Description != null)
            .ToDictionary(m => (ExampleModuleType)Enum.Parse(typeof(ExampleModuleType), m.Name, true), m => m.Description!);

    public static ExampleModule ParseError(string name) => new()
    {
        ModuleName = name,
        HtmlContent = "(error parsing code example from source xmldoc)"
    };

    public string ModuleName { get; init; } = default!;
    public ExampleModuleType ModuleType { get; init; }
    public string ModuleTypeName => ModuleTypes[ModuleType];

    public string HtmlContent { get; init; } = default!;
    public string? Description { get; init; } = default!;
}
