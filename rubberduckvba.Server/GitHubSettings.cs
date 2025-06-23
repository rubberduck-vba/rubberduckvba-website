using Hangfire;

namespace rubberduckvba.Server;

public record class ApiSettings
{
    public string SymetricKey { get; set; } = default!;
}

public record class ConnectionSettings
{
    public string RubberduckDb { get; set; } = default!;
    public string HangfireDb { get; set; } = default!;
}

public static class RDConstants
{
    public const int OrganisationId = 12832254;
    public const string WebAdminTeam = "WebAdmin";
    public const string ContributorsTeam = "Contributors";

    public const string ReaderRole = "rd-reader";
    public const string WriterRole = "rd-writer";
    public const string ReviewerRole = "rd-reviewer";
    public const string AdminRole = "rd-admin";
}

public record class GitHubSettings
{
    public string ClientId { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    public string OrgToken { get; set; } = default!;

    public string WebhookToken { get; set; } = default!;

    public string CodeInspectionDefaultSettingsUri { get; set; } = default!;

    public string UserAgent => "Rubberduck.WebApi";
    /// <summary>
    /// The owner organization name.
    /// </summary>
    public string OwnerOrg => "rubberduck-vba";
    /// <summary>
    /// The Rubberduck (2.x) repository name.
    /// </summary>
    public string Rubberduck => "Rubberduck";
    /// <summary>
    /// The Rubberduck (3.x) repository name.
    /// </summary>
    public string Rubberduck3 => "Rubberduck3";

    public int RubberduckOrgId => 12832254;

    public string JwtIssuer => "rubberduckvba.com";
    public string JwtAudience => "api.rubberduckvba.com";
}

public record class HangfireSettings
{
    public int MaxInitializationAttempts { get; set; } = 5;
    public int InitializationRetryDelaySeconds { get; set; } = 10;

    public int ServerCheckIntervalMinutes { get; set; } = 15;
    public int QueuePollIntervalSeconds { get; set; } = 30;
    public int SchedulePollIntervalSeconds { get; set; } = 30;
    public int JobExpirationCheckIntervalMinutes { get; set; } = 15;
    public int AutoRetryAttempts { get; set; } = 2;
    public int[] AutoRetryDelaySeconds { get; set; } = [2, 5, 10, 30, 60];
    public int HeartbeatIntervalMinutes { get; set; } = 15;
    public int CancellationCheckIntervalSeconds { get; set; } = 5;

    public bool ConfigureHangfireServerOnStartup { get; set; } = true;
    public int WorkerCount { get; set; } = 2;

    public bool CreateUpdateInstallerDownloadsJob { get; set; } = true;
    public string UpdateInstallerDownloadsSchedule { get; set; } = Cron.Daily();

    public bool CreateUpdateXmldocContentJob { get; set; } = true;
    public string UpdateXmldocContentSchedule { get; set; } = Cron.Never();
}
