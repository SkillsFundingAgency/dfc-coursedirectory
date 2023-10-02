using Dfc.Cosmos.JobProfileContainersToCsv.Config;
using Dfc.Cosmos.JobProfileContainersToCsv.Services;
using Microsoft.Extensions.Configuration;

namespace Dfc.Cosmos.JobProfileContainersToCsv
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var exportService = new ExportService();
            var cosmosDbConfig = GetCosmosDbConfig();
            var sqlDbConfig = GetSqlDbConfig();

            await exportService.ExportToCsvFile(cosmosDbConfig, sqlDbConfig);
        }

        private static SqlDbSettings GetSqlDbConfig()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                .Build();

            return configurationRoot.GetRequiredSection(SqlDbSettings.SectionName).Get<SqlDbSettings>();
        }

        private static CosmosDbSettings GetCosmosDbConfig()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                .Build();

            return configurationRoot.GetRequiredSection(CosmosDbSettings.SectionName).Get<CosmosDbSettings>();
        }
    }
}
