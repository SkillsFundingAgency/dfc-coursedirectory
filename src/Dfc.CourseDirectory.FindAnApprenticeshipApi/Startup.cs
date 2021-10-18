using System;
using System.Data.SqlClient;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.FindAnApprenticeshipApi;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Settings;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Dfc.CourseDirectory.FindAnApprenticeshipApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            #region Settings & Config
            
            var cosmosDbSettings = configuration.GetSection(nameof(CosmosDbSettings));
            var cosmosDbCollectionSettings = configuration.GetSection(nameof(CosmosDbCollectionSettings));
            var providerServiceSettings = configuration.GetSection(nameof(ProviderServiceSettings));
            var referenceDataServiceSettings = configuration.GetSection(nameof(ReferenceDataServiceSettings));

            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<CosmosDbSettings>(cosmosDbSettings);
            services.Configure<CosmosDbCollectionSettings>(cosmosDbCollectionSettings);
            services.Configure<ProviderServiceSettings>(providerServiceSettings);
            services.Configure<ReferenceDataServiceSettings>(referenceDataServiceSettings);

            #endregion

            #region Services

            services.AddSingleton<IProviderServiceClient, ProviderServiceClient>();
            services.AddSingleton<Func<DateTimeOffset>>(() => DateTimeOffset.UtcNow);

            services.AddSingleton<IProviderService>(s =>
            {
                var sqlConnectionString = s.GetRequiredService<IConfiguration>().GetValue<string>("ConnectionStrings:DefaultConnection");
                return new ProviderService(() => new SqlConnection(sqlConnectionString));
            });

            services.AddCosmosDbDataStore(
                endpoint: new Uri(configuration["CosmosDbSettings:EndpointUri"]),
                key: configuration["CosmosDbSettings:PrimaryKey"]);

            services.AddSingleton<IBlobStorageClient>(s =>
                new AzureBlobStorageClient(
                    new AzureBlobStorageClientOptions(
                        s.GetRequiredService<IConfiguration>().GetValue<string>("AzureWebJobsStorage"),
                        "fatp-providersexport")));

            services.AddScoped<IDASHelper, DASHelper>();
            services.AddScoped<IApprenticeshipService, ApprenticeshipService>();

            services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));

            #endregion
        }
    }
}
