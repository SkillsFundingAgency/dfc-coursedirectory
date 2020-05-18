﻿using System;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Functions;
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
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production";

            var configuration = GetConfiguration();
            builder.Services.AddSingleton(configuration);

            builder.Services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));
			
            builder.Services.AddCosmosDbDataStore(
                endpoint: new Uri(configuration["CosmosDbSettings:EndpointUri"]),
                key: configuration["CosmosDbSettings:PrimaryKey"]);

            builder.Services.AddTransient<LarsDataImporter>();
            builder.Services.AddTransient<IClock, FrozenSystemClock>();
            builder.Services.Decorate<IJobActivator, FunctionInstanceServicesCatalog>();
            builder.Services.AddSingleton(sp => (FunctionInstanceServicesCatalog)sp.GetRequiredService<IJobActivator>());

            IConfiguration GetConfiguration()
            {
                // Yuk - waiting on https://github.com/Azure/azure-webjobs-sdk/pull/2405 for a nicer way to do this
                var sp = builder.Services.BuildServiceProvider();
                var baseConfig = sp.GetRequiredService<IConfiguration>();

                var configBuilder = new ConfigurationBuilder()
                    .AddConfiguration(baseConfig);

                if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.AddUserSecrets(typeof(Startup).Assembly);
                }

                return configBuilder.Build();
            }
        }
    }
}
