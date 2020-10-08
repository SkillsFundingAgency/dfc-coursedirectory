using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Serialization;

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
        public async Task Execute(string input)
        {
            var locationIds = new HashSet<Guid>();

            await _cosmosDbQueryDispatcher.ExecuteQuery(new ProcessAllApprenticeships()
            {
                ProcessChunk = async chunk =>
                {
                    foreach (var apprenticeship in chunk)
                    {
                        var duplicateLocationIds = new List<Guid>();

                        foreach (var location in apprenticeship.ApprenticeshipLocations)
                        {
                            if (!locationIds.Add(location.Id))
                            {
                                duplicateLocationIds.Add(location.Id);
                            }
                        }

                        if (duplicateLocationIds.Any())
                        {
                            await _cosmosDbQueryDispatcher.ExecuteQuery(
                                new ReallocateDuplicateApprenticeshipLocationIds()
                                {
                                    Apprenticeship = apprenticeship,
                                    DuplicateLocationIds = duplicateLocationIds,
                                    UpdatedBy = nameof(FixDuplicateApprenticeshipLocationIds),
                                    UpdatedOn = _clock.UtcNow
                                });
                            Debugger.Break();
                        }
                    }
                }
            });
        }
    }
}
