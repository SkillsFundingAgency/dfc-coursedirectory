using System;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core;
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
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Dfc.CourseDirectory.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));
                        
            builder.Services.AddTransient<LarsDataImporter>();
            builder.Services.Configure<LarsDataset>(
                configuration.GetSection(nameof(LarsDataset)));
            builder.Services.AddTransient<IUkrlpWcfClientFactory, UkrlpWcfClientFactory>();
            builder.Services.AddTransient<IClock, SystemClock>();
            builder.Services.AddSingleton<FunctionInstanceServicesCatalog>();            

            builder.Services.AddSingleton<CommitSqlTransactionMiddleware>();           

            builder.Services.AddTransient<IUkrlpService, Core.ReferenceData.Ukrlp.UkrlpService>();
            builder.Services.AddTransient<UkrlpSyncHelper>();
            builder.Services.AddTransient<OnspdDataImporter>();
            builder.Services.AddSingleton<IRegionCache, RegionCache>();
            builder.Services.Configure<GoogleWebRiskSettings>(
                configuration.GetSection(nameof(GoogleWebRiskSettings)));
            builder.Services.AddScoped<IWebRiskService, WebRiskService>();
            builder.Services.AddTransient<IFileUploadProcessor, FileUploadProcessor>();
            builder.Services.AddSingleton<CampaignDataImporter>();

            builder.Services.AddSingleton(
                _ => new BlobServiceClient(configuration["BlobStorageSettings:ConnectionString"]));
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production";

            if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
            {
                builder.ConfigurationBuilder.AddUserSecrets(typeof(Startup).Assembly);
            }

            builder.ConfigurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "CampaignDataContainerName", "campaign-data" },
                { "DataUploadsContainerName", Constants.ContainerName },
                { "CourseUploadsFolderName", Constants.CoursesFolder },
                { "VenueUploadsFolderName", Constants.VenuesFolder }
            });
        }
    }
}
