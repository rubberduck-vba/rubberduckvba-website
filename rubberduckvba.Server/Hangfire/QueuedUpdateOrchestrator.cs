using Hangfire.Server;
using Microsoft.Extensions.Options;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using RubberduckServices;
using rubberduckvba.Server.ContentSynchronization;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Services;
using System.Diagnostics;

namespace rubberduckvba.Server.Hangfire;

public static class QueuedUpdateOrchestrator
{
    public static async Task UpdateInstallerDownloadStats(TagSyncRequestParameters parameters, PerformContext context)
        => await Run(Configure<InstallerDownloadStatsOrchestrator, TagSyncRequestParameters>(), context, parameters).ConfigureAwait(false);

    public static async Task UpdateXmldocContent(XmldocSyncRequestParameters parameters, PerformContext context)
        => await Run(Configure<XmldocContentOrchestrator, XmldocSyncRequestParameters>(), context, parameters).ConfigureAwait(false);

    private static IServiceCollection Configure<TOrchestrator, TParameters>()
        where TOrchestrator : class, IContentOrchestrator<TParameters>
        where TParameters : SyncRequestParameters
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            //.AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();


        services.Configure<ConnectionSettings>(options =>
        {
            var section = configuration.GetSection("ConnectionStrings");
            section.Bind(options);

            if (string.IsNullOrWhiteSpace(options.RubberduckDb) || string.IsNullOrWhiteSpace(options.HangfireDb))
            {
                var rddb = configuration.GetConnectionString("RubberduckDb");
                var hfdb = configuration.GetConnectionString("HangfireDb");

                options.RubberduckDb = rddb ?? throw new InvalidOperationException("Could not retrieve 'RubberduckDb' connection string from configuration.");
                options.HangfireDb = hfdb ?? throw new InvalidOperationException("Could not retrieve 'HangfireDb' connection string from configuration.");
            }
        });
        services.Configure<GitHubSettings>(options => configuration.GetSection("GitHub").Bind(options));
        services.Configure<ApiSettings>(options => configuration.GetSection("Api").Bind(options));

        ConfigureLogging(services);
        ConfigureServices(services);

        services.AddSingleton<IContentOrchestrator<TParameters>, TOrchestrator>();

        return services;
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            var config = new LoggingConfiguration();

            (NLog.LogLevel MinLevel, string, Target Target)[] targets = [
                (NLog.LogLevel.Trace, "*", new DebuggerTarget("DebuggerLog")),
                (NLog.LogLevel.Info, "*", new EventLogTarget("EventLog")
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

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMarkdownFormattingService, MarkdownFormattingService>();
        services.AddSingleton<ISyntaxHighlighterService, SyntaxHighlighterService>();

        services.AddSingleton<IRubberduckDbService, RubberduckDbService>();
        services.AddSingleton<IGitHubClientService, GitHubClientService>();
        services.AddSingleton<ISynchronizationPipelineFactory<SyncContext>, SynchronizationPipelineFactory>();
        services.AddSingleton<IXmlDocMerge, XmlDocMerge>();
        services.AddSingleton<IStagingServices, StagingServices>();
    }

    private static async Task Run<TParameters>(IServiceCollection services, PerformContext context, TParameters parameters)
        where TParameters : SyncRequestParameters
    {
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<IContentOrchestrator<TParameters>>>();

        var configuration = provider.GetRequiredService<IOptions<ConnectionSettings>>();
        var service = provider.GetRequiredService<IContentOrchestrator<TParameters>>();

        LogStart(logger, context);
        context.CancellationToken.ThrowIfCancellationRequested();

        await service.UpdateContentAsync(parameters with { JobId = context.BackgroundJob.Id }, new HangfireTokenSource(context.CancellationToken));
    }

    private static void LogStart(ILogger logger, PerformContext context)
    {
        const string message = "** Job started {0}: Id {1} ({2}), created {3}.";
        var jobStart = TimeProvider.System.GetUtcNow();
        var jobId = context.BackgroundJob.Id;
        var jobQueue = context.BackgroundJob.Job.Queue;
        var jobCreated = context.BackgroundJob.CreatedAt;

        logger?.LogInformation(message, jobStart.ToTimestampString(), jobId, jobQueue, jobCreated.ToTimestampString());
        Debug.WriteLine(string.Format(message, jobStart, jobId, jobQueue, jobCreated));
    }
}
