namespace rubberduckvba.Server.Api.Admin;

public readonly record struct GitRef
{
    private readonly string _value;

    public GitRef(string value)
    {
        _value = value;
        IsTag = value?.StartsWith("refs/tags/") ?? false;
        Name = value?.Split('/').Last() ?? string.Empty;
    }

    public bool IsTag { get; }
    public string Name { get; }

    public override string ToString() => _value;
}
