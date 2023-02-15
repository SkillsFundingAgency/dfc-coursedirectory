using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CosmosToCsv.Config;
using Dfc.CosmosToCsv.Data;
using Dfc.CosmosToCsv.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Dfc.CosmosToCsv
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configs = GetConfig();

            foreach (var config in configs)
            {
                var exportService = new ExportService(config.EndpointUrl, config.AccessKey);

                var request = new ExportRequest
                {
                    DatabaseId = config.DatabaseId,
                    ContainerId = config.ContainerId,
                    Query = new QueryDefinition(config.Query)
                };

                var results = await exportService.ExportData(request);

                Console.WriteLine("Found {0} records", results.Count);

                exportService.ExportToFile(results, config.Key, config.Filename);
            }
        }

        private static IEnumerable<CosmosDbSettings> GetConfig()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                .Build();

            return configurationRoot.GetRequiredSection(CosmosDbSettings.SectionName)
                .Get<List<CosmosDbSettings>>();
        }
    }
}
