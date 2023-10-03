using Dfc.Cosmos.JobProfileContainersToCsv.Config;
using Dfc.Cosmos.JobProfileContainersToCsv.Services;
using Microsoft.Extensions.Configuration;

namespace Dfc.Cosmos.JobProfileContainersToCsv
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var appSettings = GetCosmosDbConfig();

            var exportService = new ExportService();
            await exportService.ExportToCsvFile(appSettings);
        }       

        private static AppSettings GetCosmosDbConfig()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                .Build();

            return configurationRoot.Get<AppSettings>();
        }
    }
}
