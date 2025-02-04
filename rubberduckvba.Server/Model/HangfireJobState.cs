namespace rubberduckvba.Server.Model;

public record class HangfireJobState
{
    public string JobName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public int? LastJobId { get; set; }
    public string? NextExecution { get; set; }
    public string? StateName { get; set; }
    public string? StateTimestamp { get; set; }
}
