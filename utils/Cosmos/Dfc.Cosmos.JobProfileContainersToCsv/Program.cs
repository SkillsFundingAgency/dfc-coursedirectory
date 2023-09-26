using Dfc.Cosmos.JobProfileContainersToCsv.Config;
using Dfc.Cosmos.JobProfileContainersToCsv.Data;
using Dfc.Cosmos.JobProfileContainersToCsv.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Dfc.Cosmos.JobProfileContainersToCsv
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configs = GetConfig();
            await ExportToCsvFile(configs);
        }

        private static async Task ExportToCsvFile(IEnumerable<CosmosDbSettings> configs)
        {
            List<IList<JObject>> cosmosContainersData = new List<IList<JObject>>();

            foreach (var config in configs)
            {
                var exportService = new ExportService(config.EndpointUrl, config.AccessKey);

                foreach (var containerSettings in config.Containers)
                {
                    var request = new ExportRequest
                    {
                        DatabaseId = config.DatabaseId,
                        ContainerId = containerSettings.ContainerId,
                        Query = new QueryDefinition(containerSettings.Query)
                    };

                    var cosmosData = await exportService.GetCosmosData(request);
                    cosmosContainersData.Add(cosmosData);

                    Console.WriteLine("Found {0} records for container {1}", cosmosData.Count, containerSettings.ContainerId);
                }

                exportService.ExportToFile(cosmosContainersData, config.Key, config.Filename);

                Console.WriteLine("Job Profiles' data exported to file {0} successfully.", config.Filename);
            }
        }

        private static IEnumerable<CosmosDbSettings> GetConfig()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                .Build();

            return configurationRoot.GetRequiredSection(CosmosDbSettings.SectionName).Get<List<CosmosDbSettings>>();
        }
    }
}
