namespace rubberduckvba.Server.Model;

public record class AnnotationParameter
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string Description { get; init; } = string.Empty;
}
