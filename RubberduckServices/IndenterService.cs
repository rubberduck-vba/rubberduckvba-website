using Rubberduck.SmartIndenter;
using RubberduckServices.Internal;

namespace RubberduckServices;

public interface IIndenterService
{
    string IndenterVersion();
    Task<string[]> IndentAsync(IIndenterSettings settings);
}

public class IndenterService : IIndenterService
{
    private readonly ISimpleIndenter _indenter;

    public IndenterService(ISimpleIndenter indenter)
    {
        _indenter = indenter;
    }

    public string IndenterVersion()
    {
        return typeof(Rubberduck.SmartIndenter.IIndenterSettings).Assembly.GetName().Version.ToString(3);
    }

    public async Task<string[]> IndentAsync(IIndenterSettings settings)
    {
        var adapter = new IndenterSettingsAdapter(settings);
        return await Task.FromResult(_indenter.Indent(adapter.Code, adapter).ToArray());
    }
}
