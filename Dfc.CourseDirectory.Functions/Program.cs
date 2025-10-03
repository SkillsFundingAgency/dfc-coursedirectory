using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.ReferenceData.Campaigns;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));

        services.AddTransient<LarsDataImporter>();
        services.Configure<LarsDataset>(
            configuration.GetSection(nameof(LarsDataset)));
        services.AddTransient<IUkrlpWcfClientFactory, UkrlpWcfClientFactory>();
        services.AddTransient<IClock, SystemClock>();
        services.AddSingleton<FunctionInstanceServicesCatalog>();

        services.AddSingleton<CommitSqlTransactionMiddleware>();

        services.AddSingleton<QueueBackgroundWorkScheduler>();
        services.AddSingleton<IBackgroundWorkScheduler>(sp => sp.GetRequiredService<QueueBackgroundWorkScheduler>());
        services.AddHttpClient();

        services.AddTransient<IUkrlpService, Dfc.CourseDirectory.Core.ReferenceData.Ukrlp.UkrlpService>();
        services.AddTransient<UkrlpSyncHelper>();
        services.AddTransient<OnspdDataImporter>();
        services.AddSingleton<IRegionCache, RegionCache>();
        services.Configure<GoogleWebRiskSettings>(
            configuration.GetSection(nameof(GoogleWebRiskSettings)));
        services.AddScoped<IWebRiskService, WebRiskService>();
        services.AddTransient<IFileUploadProcessor, FileUploadProcessor>();
        //services.AddSingleton<CampaignDataImporter>();

        services.AddSingleton(
            _ => new BlobServiceClient(configuration["BlobStorageSettings:ConnectionString"]));
        services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production";

        if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            config.AddUserSecrets(typeof(Program).Assembly);
        }
    })
    .Build();

host.Run();
