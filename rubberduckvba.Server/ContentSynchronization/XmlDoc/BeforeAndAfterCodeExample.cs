using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.XmlDoc;

public class BeforeAndAfterCodeExample
{
    public BeforeAndAfterCodeExample(IEnumerable<ExampleModule> modulesBefore, IEnumerable<ExampleModule> modulesAfter)
    {
        ModulesBefore = modulesBefore ?? Enumerable.Empty<ExampleModule>();
        ModulesAfter = modulesAfter ?? Enumerable.Empty<ExampleModule>();
    }

    public IEnumerable<ExampleModule> ModulesBefore { get; }
    public IEnumerable<ExampleModule> ModulesAfter { get; }

    public Example AsExample(string description = "", int sortOrder = 0) => new QuickFixExample()
    {
        Description = description,
        SortOrder = sortOrder,
        ModulesBefore = ModulesBefore.ToList(),
        ModulesAfter = ModulesAfter.ToList()
    };
}
