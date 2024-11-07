namespace rubberduckvba.Server.Model.Entity;

public abstract record class Entity
{
    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public string Name { get; init; } = string.Empty;
}
