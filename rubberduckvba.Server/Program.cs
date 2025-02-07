using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using RubberduckServices;
using rubberduckvba.Server.Api.Admin;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Hangfire;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;
using rubberduckvba.Server.Services.rubberduckdb;
using System.Diagnostics;
using System.Reflection;

namespace rubberduckvba.Server;

public class HangfireAuthenticationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context) => Debugger.IsAttached || context.Request.RemoteIpAddress == "20.220.30.154";
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

        builder.Services.Configure<ConnectionSettings>(options => builder.Configuration.GetSection("ConnectionStrings").Bind(options));
        builder.Services.Configure<GitHubSettings>(options => builder.Configuration.GetSection("GitHub").Bind(options));
        builder.Services.Configure<ApiSettings>(options => builder.Configuration.GetSection("Api").Bind(options));
        builder.Services.Configure<HangfireSettings>(options => builder.Configuration.GetSection("Hangfire").Bind(options));

        builder.Services.AddAuthentication(options =>
        {
            options.RequireAuthenticatedSignIn = false;
            options.DefaultAuthenticateScheme = "github";

            options.AddScheme("github", builder =>
            {
                builder.HandlerType = typeof(GitHubAuthenticationHandler);
            });
            options.AddScheme("webhook-signature", builder =>
            {
                builder.HandlerType = typeof(WebhookAuthenticationHandler);
            });
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("github", builder =>
            {
                builder.RequireAuthenticatedUser().AddAuthenticationSchemes("github");
            });
            options.AddPolicy("webhook", builder =>
            {
                builder.RequireAuthenticatedUser().AddAuthenticationSchemes("webhook-signature");
            });
        });

        ConfigureServices(builder.Services);
        ConfigureLogging(builder.Services);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureHangfire(builder.Services, builder.Configuration);

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapFallbackToFile("/index.html");

        app.UseCors(policy =>
        {
            policy
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(origin => true);
        });

        StartHangfire(app);
        app.Run();
    }

    private static void StartHangfire(WebApplication app)
    {
        var hangfireOptions = app.Services.GetService<IOptions<HangfireSettings>>()?.Value ?? new();
        if (hangfireOptions.ConfigureHangfireServerOnStartup)
        {
            var scheduler = app.Services.GetRequiredService<IRecurringJobManagerV2>();

            if (hangfireOptions.CreateUpdateInstallerDownloadsJob)
            {
                scheduler.AddOrUpdate(
                    HangfireConstants.UpdateInstallerDownloadsJobId,
                    HangfireConstants.ScheduledQueueName,
                    () => QueuedUpdateOrchestrator.UpdateInstallerDownloadStats(
                        new TagSyncRequestParameters
                        {
                            RepositoryId = RepositoryId.Rubberduck,
                            RequestId = Guid.NewGuid()
                        }, null!),
                    hangfireOptions.UpdateInstallerDownloadsSchedule);
            }

            if (hangfireOptions.CreateUpdateXmldocContentJob)
            {
                scheduler.AddOrUpdate(
                    HangfireConstants.UpdateXmldocContentJobId,
                    HangfireConstants.ManualQueueName,
                    () => QueuedUpdateOrchestrator.UpdateXmldocContent(
                        new XmldocSyncRequestParameters
                        {
                            RepositoryId = RepositoryId.Rubberduck,
                            RequestId = Guid.NewGuid()
                        }, null!),
                    hangfireOptions.UpdateXmldocContentSchedule);
            }

            app.UseHangfireDashboard(HangfireConstants.DashboardUrl, new DashboardOptions { DarkModeEnabled = true, Authorization = [new HangfireAuthenticationFilter()] }, scheduler.Storage);
            CleanStartHangfire(scheduler.Storage);
        }
    }

    private static void ConfigureSession(SessionOptions options)
    {
        options.Cookie.Name = "rubberduckvba.session";
        options.Cookie.IsEssential = true;

        options.IdleTimeout = TimeSpan.FromMinutes(50);
        options.IOTimeout = Timeout.InfiniteTimeSpan;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ConfigurationOptions>();
        services.AddSingleton<IMarkdownFormattingService, MarkdownFormattingService>();
        services.AddSingleton<ISyntaxHighlighterService, SyntaxHighlighterService>();
        services.AddSingleton<WebhookHeaderValidationService>();
        services.AddSingleton<WebhookPayloadValidationService>();
        services.AddSingleton<WebhookSignatureValidationService>();
        services.AddSingleton<HangfireLauncherService>();

        services.AddSingleton<IRubberduckDbService, RubberduckDbService>();
        services.AddSingleton<TagServices>();
        services.AddSingleton<FeatureServices>();
        services.AddSingleton<IRepository<TagEntity>, TagsRepository>();
        services.AddSingleton<IRepository<TagAssetEntity>, TagAssetsRepository>();
        services.AddSingleton<IRepository<FeatureEntity>, FeaturesRepository>();
        services.AddSingleton<IRepository<InspectionEntity>, InspectionsRepository>();
        services.AddSingleton<IRepository<QuickFixEntity>, QuickFixRepository>();
        services.AddSingleton<IRepository<AnnotationEntity>, AnnotationsRepository>();
        services.AddSingleton<HangfireJobStateRepository>();

        services.AddSingleton<IGitHubClientService, GitHubClientService>();
        services.AddSingleton<IContentOrchestrator<TagSyncRequestParameters>, InstallerDownloadStatsOrchestrator>();
        services.AddSingleton<IContentOrchestrator<XmldocSyncRequestParameters>, XmldocContentOrchestrator>();
        services.AddSingleton<ISynchronizationPipelineFactory<SyncContext>, SynchronizationPipelineFactory>();
        services.AddSingleton<IXmlDocMerge, XmlDocMerge>();
        services.AddSingleton<IStagingServices, StagingServices>();
        services.AddSingleton<XmlDocAnnotationParser>();
        services.AddSingleton<XmlDocQuickFixParser>();
        services.AddSingleton<XmlDocInspectionParser>();

        services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
        services.AddSingleton<CacheService>();

        services.AddSession(ConfigureSession);
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            var config = new LoggingConfiguration();

            (NLog.LogLevel MinLevel, string, Target Target)[] targets = [
                (NLog.LogLevel.Trace, "rubberduckvba.*", new DebuggerTarget("DebuggerLog")),
                (NLog.LogLevel.Info, "rubberduckvba.*", new FileTarget("FileLog")
                {
                    FileName = "logs/rubberduckvba.Server.log",
                    DeleteOldFileOnStartup = true,
                    CreateDirs = true,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveOldFileOnStartup = true,
                    EnableFileDelete = true,
                    MaxArchiveDays = 10,
                }),
                (NLog.LogLevel.Error, "*", new EventLogTarget("EventLog")
                {
                    Source = "rubberduckvba.Server",
                    Layout = "${message}",
                    Log = "Application",
                    EventId = "${event-properties:EventID:whenEmpty=0}"
                }),
            ];

            config.LoggingRules.Clear();
            foreach (var (minLevel, pattern, target) in targets)
            {
                var maxLevel = minLevel == NLog.LogLevel.Off
                    ? NLog.LogLevel.Off
                    : NLog.LogLevel.Fatal;

                config.AddTarget(target);
                config.AddRule(minLevel, maxLevel, target, pattern);
            }

            builder.AddNLog(config);
        });
    }

    private static void ConfigureHangfire(IServiceCollection services, IConfiguration configuration)
    {
        var hangfireSettings = new HangfireSettings();
        configuration.Bind("Hangfire", hangfireSettings);

        if (!hangfireSettings.ConfigureHangfireServerOnStartup)
        {
            return;
        }

        var connections = new ConnectionSettings();
        configuration.Bind("ConnectionStrings", connections);

        services.AddHangfire((options) =>
        {
            var storageOptions = new SqlServerStorageOptions
            {
                SchemaName = "hangfire",
                CountersAggregateInterval = TimeSpan.FromMinutes(15),
                JobExpirationCheckInterval = TimeSpan.FromMinutes(hangfireSettings.JobExpirationCheckIntervalMinutes),
                QueuePollInterval = TimeSpan.FromSeconds(hangfireSettings.QueuePollIntervalSeconds),
                PrepareSchemaIfNecessary = true
            };

            options.UseSqlServerStorage(connections.HangfireDb, storageOptions);
            options.UseFilter(new AutomaticRetryAttribute { Attempts = hangfireSettings.AutoRetryAttempts, DelaysInSeconds = hangfireSettings.AutoRetryDelaySeconds });
        });

        services.AddHangfireServer(options =>
        {
            options.ServerName = HangfireConstants.ServerName;
            options.Queues = [HangfireConstants.ScheduledQueueName, HangfireConstants.ManualQueueName];
            options.WorkerCount = hangfireSettings.WorkerCount;
            options.HeartbeatInterval = TimeSpan.FromMinutes(hangfireSettings.HeartbeatIntervalMinutes);
            options.SchedulePollingInterval = TimeSpan.FromSeconds(hangfireSettings.SchedulePollIntervalSeconds);
            options.CancellationCheckInterval = TimeSpan.FromSeconds(hangfireSettings.CancellationCheckIntervalSeconds);
            options.ServerCheckInterval = TimeSpan.FromMinutes(hangfireSettings.ServerCheckIntervalMinutes);
        });
    }

    private static void CleanStartHangfire(JobStorage storage)
    {
        var api = storage.GetMonitoringApi();
        var connection = storage.GetConnection();

        var servers = api.Servers();
        foreach (var server in servers)
        {
            if (server.Name.Contains(HangfireConstants.ServerName))
            {
                connection.RemoveServer(server.Name);
            }
        }
    }
}

public static class LoggerExtensions
{
    public static EventId LogMessageEventId { get; } = new EventId(56781, "LogEvent");
    public static EventId LogErrorEventId { get; } = new EventId(56789, "LogErrorEvent");

    public static void Log(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.LogLevel level, IRequestParameters context, string template, params object[] args)
        => LoggerMessage.Define(level, LogMessageEventId, $"🏷️ {context.RequestId}:[{context.JobId}] | {string.Format(template, args)}").Invoke(logger, null);

    public static void LogTrace(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, string template, params object[] args)
        => Log(logger, Microsoft.Extensions.Logging.LogLevel.Trace, context, template, args);

    public static void LogDebug(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, string template, params object[] args)
        => Log(logger, Microsoft.Extensions.Logging.LogLevel.Debug, context, template, args);

    public static void LogInformation(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, string template, params object[] args)
        => Log(logger, Microsoft.Extensions.Logging.LogLevel.Information, context, template, args);

    public static void LogWarning(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, string template, params object[] args)
        => Log(logger, Microsoft.Extensions.Logging.LogLevel.Warning, context, template, args);

    public static void LogError(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, string template, params object[] args)
        => Log(logger, Microsoft.Extensions.Logging.LogLevel.Error, context, template, args);

    public static void LogException(this Microsoft.Extensions.Logging.ILogger logger, IRequestParameters context, Exception exception)
        => LoggerMessage.Define(
            logLevel: Microsoft.Extensions.Logging.LogLevel.Error,
            eventId: LogErrorEventId,
            formatString: $"🏷️ {context.RequestId}:[{context.JobId}] | ❌ {exception.GetType().Name} | {exception.Message} | {exception.StackTrace}").Invoke(logger, exception);
}
