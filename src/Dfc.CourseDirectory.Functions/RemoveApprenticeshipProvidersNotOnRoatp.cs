using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class RemoveApprenticeshipProvidersNotOnRoatp
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public RemoveApprenticeshipProvidersNotOnRoatp(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        [FunctionName(nameof(RemoveApprenticeshipProvidersNotOnRoatp))]
        [NoAutomaticTrigger]
        public async Task Execute(string input)
        {
            using var listStream = typeof(RemoveApprenticeshipProvidersNotOnRoatp).Assembly.GetManifestResourceStream("Dfc.CourseDirectory.Functions.UkprnsNotOnRoatp.csv");
            using var reader = new StreamReader(listStream);

            string line;
            var lines = new List<string>();

            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            var ukprnsToRemoveProviderStatusFrom = new HashSet<int>(lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => int.Parse(line)));

            await _cosmosDbQueryDispatcher.ExecuteQuery(new ProcessAllProviders()
            {
                ProcessChunk = async providers =>
                {
                    foreach (var provider in providers)
                    {
                        if (provider.ProviderType.HasFlag(Core.Models.ProviderType.Apprenticeships) &&
                            ukprnsToRemoveProviderStatusFrom.Contains(provider.Ukprn))
                        {
                            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
                            {
                                ProviderId = provider.Id,
                                ProviderType = provider.ProviderType & (~Core.Models.ProviderType.Apprenticeships)
                            });
                        }
                    }
                }
            });
        }
    }
}
