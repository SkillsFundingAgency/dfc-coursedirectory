using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class FixDuplicateApprenticeshipLocationIds
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;

        public FixDuplicateApprenticeshipLocationIds(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
        }

        [FunctionName(nameof(FixDuplicateApprenticeshipLocationIds))]
        [NoAutomaticTrigger]
        public Task Execute(string input) => _cosmosDbQueryDispatcher.ExecuteQuery(new ProcessAllApprenticeships()
        {
            ProcessChunk = async chunk =>
            {
                foreach (var apprenticeship in chunk)
                {
                    var hasDuplicates = apprenticeship.ApprenticeshipLocations
                        .GroupBy(l => l.Id)
                        .Any(g => g.Count() > 1);

                    if (hasDuplicates)
                    {
                        await _cosmosDbQueryDispatcher.ExecuteQuery(
                            new ReallocateDuplicateApprenticeshipLocationIds()
                            {
                                Apprenticeship = apprenticeship,
                                UpdatedBy = nameof(FixDuplicateApprenticeshipLocationIds),
                                UpdatedOn = _clock.UtcNow
                            });
                    }
                }
            }
        });
    }
}
