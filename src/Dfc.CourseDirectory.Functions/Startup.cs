﻿using System;
using Azure;
using Azure.Search.Documents;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Functions;
using Dfc.CourseDirectory.Functions.Config;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
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

            builder.Services.AddCosmosDbDataStore(
                endpoint: new Uri(configuration["CosmosDbSettings:EndpointUri"]),
                key: configuration["CosmosDbSettings:PrimaryKey"]);

            builder.Services.AddTransient<LarsDataImporter>();
            builder.Services.AddTransient<IUkrlpWcfClientFactory, UkrlpWcfClientFactory>();
            builder.Services.AddTransient<IClock, FrozenSystemClock>();
            builder.Services.Decorate<IJobActivator, FunctionInstanceServicesCatalog>();
            builder.Services.AddSingleton(sp => (FunctionInstanceServicesCatalog)sp.GetRequiredService<IJobActivator>());
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Services.AddSingleton<IFunctionFilter, CommitSqlTransactionFunctionInvocationFilter>();
#pragma warning restore CS0618 // Type or member is obsolete
            builder.Services.AddTransient<IUkrlpService, Core.ReferenceData.Ukrlp.UkrlpService>();
            builder.Services.AddTransient<UkrlpSyncHelper>();
            builder.Services.AddTransient<SqlDataSync>();
            builder.Services.AddHttpClient<LarsDataImporter>();

            builder.Services.AddSingleton<Func<SearchClientSettings, SearchClient>>(settings =>
                new SearchClient(new Uri(settings.Url), settings.IndexName, new AzureKeyCredential(settings.Key)));
        }


        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production";

            if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
            {
                builder.ConfigurationBuilder.AddUserSecrets(typeof(Startup).Assembly);
            }
        }
    }
}
