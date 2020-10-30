using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Functions
{
    public class ArchiveApprenticeships
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public ArchiveApprenticeships(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [FunctionName("ArchiveApprenticeships")]
        [NoAutomaticTrigger]
        public async Task Run(string apprenticeshipIdsJson, ILogger log)
        {
            if (apprenticeshipIdsJson == null)
            {
                throw new ArgumentNullException(nameof(apprenticeshipIdsJson));
            }

            var apprenticeshipIds = JsonConvert.DeserializeObject<Guid[]>(apprenticeshipIdsJson);

            if (apprenticeshipIds == null)
            {
                throw new ArgumentException($"Failed to parse {nameof(apprenticeshipIdsJson)}.", nameof(apprenticeshipIdsJson));
            }

            log.LogInformation($"{nameof(ArchiveApprenticeships)} executing for {apprenticeshipIds.Length} apprenticeships.");

            // Chunk up the requests so as to not exceed the query size limits
            var apprenticeships = (
                await Task.WhenAll(apprenticeshipIds
                    .Select((id, i) => new { ApprenticeshipId = id, Index = i })
                    .GroupBy(x => x.Index / 500)
                    .Select(async x =>
                    {
                        var a = await _cosmosDbQueryDispatcher.ExecuteQuery(
                            new GetApprenticeshipsByIds { ApprenticeshipIds = x.Select(v => v.ApprenticeshipId) });

                        return a.Values;
                    })))
                .SelectMany(a => a)
                .ToArray();

            var results = new List<OneOf<Success, NotFound, Error>>();

            foreach (var notFoundApprenticeshipId in apprenticeshipIds.Except(apprenticeships.Select(a => a.Id)))
            {
                results.Add(new NotFound());
                log.LogWarning($"Failed to archive {notFoundApprenticeshipId} - not found.");
            }

            foreach (var a in apprenticeships)
            {
                try
                {
                    var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipStatus
                    {
                        ApprenticeshipId = a.Id,
                        ProviderUkprn = a.ProviderUKPRN,
                        Status = 4
                    });

                    results.Add(result.Match<OneOf<Success, NotFound, Error>>(
                        r =>
                        {
                            log.LogWarning($"Failed to archive {a.Id} - not found.");
                            return new NotFound();
                        },
                        r =>
                        {
                            log.LogInformation($"Successfully archived {a.Id}.");
                            return new Success();
                        }));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Failed to archive {a.Id} - exception.");
                    results.Add(new Error());
                }
            }

            log.LogInformation($"{nameof(ArchiveApprenticeships)} completed. {results.Count(r => r.IsT0)} succeeded. {results.Count(r => r.IsT1)} not found. {results.Count(r => r.IsT2)} errors.");
        }
    }
}
