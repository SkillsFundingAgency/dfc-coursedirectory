using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
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

            var ukprnsToRemoveProviderTypeFrom = new HashSet<int>(lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => int.Parse(line)));

            foreach (var ukprn in ukprnsToRemoveProviderTypeFrom)
            {
                var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });

                if (provider.ProviderType.HasFlag(ProviderType.Apprenticeships))
                {
                    await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
                    {
                        ProviderId = provider.Id,
                        ProviderType = provider.ProviderType & (~ProviderType.Apprenticeships)
                    });
                }
            }
        }
    }
}
