namespace rubberduckvba.Server.Model;

public record class HangfireJobState
{
    public string JobName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public int? LastJobId { get; set; }
    public DateTime? NextExecution { get; set; }
    public string? StateName { get; set; }
    public DateTime? StateTimestamp { get; set; }
}
