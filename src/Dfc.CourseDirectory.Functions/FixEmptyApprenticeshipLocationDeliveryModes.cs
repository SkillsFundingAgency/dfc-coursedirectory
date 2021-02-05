using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class FixEmptyApprenticeshipLocationDeliveryModes
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public FixEmptyApprenticeshipLocationDeliveryModes(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        [FunctionName(nameof(AnalyseFixEmptyApprenticeshipLocationDeliveryModes))]
        [NoAutomaticTrigger]
        public async Task AnalyseFixEmptyApprenticeshipLocationDeliveryModes(string input, ILogger logger)
        {
            var count = 0;

            await ProcessApprenticeshipLocations((apprenticeship, location) =>
            {
                Interlocked.Increment(ref count);

                logger.LogInformation($"{nameof(AnalyseFixEmptyApprenticeshipLocationDeliveryModes)} found apprenticeship: {apprenticeship.Id}/{location.Id}.");

                return Task.CompletedTask;
            });

            logger.LogInformation($"{nameof(AnalyseFixEmptyApprenticeshipLocationDeliveryModes)} found {count} affected apprenticeship locations.");
        }

        [FunctionName(nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes))]
        [NoAutomaticTrigger]
        public async Task ExecuteFixEmptyApprenticeshipLocationDeliveryModes(string input, ILogger logger)
        {
            var successCount = 0;
            var failedCount = 0;
            var errorCount = 0;

            await ProcessApprenticeshipLocations(async (apprenticeship, location) =>
            {
                try
                {
                    var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipLocationDeliveryMode
                    {
                        ApprenticeshipId = apprenticeship.Id,
                        ProviderUkprn = apprenticeship.ProviderUKPRN,
                        ApprenticeshipLocationId = location.Id,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress }
                    });

                    result.Switch(
                        _ =>
                        {
                            logger.LogWarning($"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} failed to update apprenticeship: {apprenticeship.Id}/{location.Id}.");
                            Interlocked.Increment(ref failedCount);
                        },
                        _ =>
                        {
                            logger.LogInformation($"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} successfully updated apprenticeship: {apprenticeship.Id}/{location.Id}.");
                            Interlocked.Increment(ref successCount);
                        });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} failed with exception.");
                    Interlocked.Increment(ref errorCount);
                }
            });

            logger.LogInformation($"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} successfully fixed {successCount} apprenticeship locations.");

            if (failedCount > 0)
            {
                logger.LogWarning($"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} failed to fix {failedCount} apprenticeship locations.");
            }
            
            if (errorCount > 0)
            {
                logger.LogWarning($"{nameof(ExecuteFixEmptyApprenticeshipLocationDeliveryModes)} failed with exception for {errorCount} apprenticeship locations.");
            }
        }

        private Task ProcessApprenticeshipLocations(Func<Apprenticeship, ApprenticeshipLocation, Task> process) =>
            _cosmosDbQueryDispatcher.ExecuteQuery(new ProcessAllApprenticeships
            {
                Predicate = a => a.RecordStatus != (int)ApprenticeshipStatus.Archived
                    && a.RecordStatus != (int)ApprenticeshipStatus.Deleted
                    && a.ApprenticeshipLocations != null
                    && a.ApprenticeshipLocations.Any(l =>
                        l != null
                        && l.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased
                        && (l.DeliveryModes == null || !l.DeliveryModes.Any())),
                ProcessChunk = async chunk =>
                {
                    foreach (var (a, l) in chunk
                        .SelectMany(c => c.ApprenticeshipLocations.Where(l => l != null
                            && l.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased
                            && (l.DeliveryModes == null || !l.DeliveryModes.Any())), (a, l) => (a, l)))
                    {
                        await process(a, l);
                    }
                }
            });
    }
}
