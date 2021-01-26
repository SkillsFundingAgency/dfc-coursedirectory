using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public class FixApprenticeshipVenueReferences
    {
        /* This fixup logic is based on ruby analysis of a production backup
         * https://gist.github.com/timabell/f8b429e87a6856f18f6664c7d6519287#file-ncs-analyse-appr-venues-rb
         *
         * The data hierarchy is:
         *     provider >> apprenticeships >> locations -> location.venueid -> venue
         * The provider also owns the list of venues:
         *     provider >> venues
         *
         * Apprenticeships live in their own cosmosdb collection as individual documents containing locations and a
         * cache of the venue address which we use here to reconnect the broken foreign key to venue.
         *
         * We have since writing this function [re]discovered LocationGuidId is actually also a VenueId and
         * has similar but not identical data to that produced by this heuristics based function. Dealing with
         * the VenueId/LocationGuidId split will be a task for another day. */

        private const string LogPrefix = "ApprenticeshipVenueFixup:";
        private const int BatchSize = 250; // the default of ~1500 resulted in timeouts waiting for our code to run; 500 makes it slow to shut down when cancelled
        private const string ReportBlobContainer = "venue-fixup-2021";

        /// <summary>
        /// Suffix for blob storage connection string key for the blob storage to upload reports to.
        /// Gotcha: this key gets prefixed with "AzureWebJobs" before being looked up in settings. I don't know why.
        /// Use same blob storage key as the functions runtime (AzureWebJobsStorage) as it's already configured to
        /// the right place.
        /// </summary>
        private const string BlobConnectionStringKey = "Storage";

        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly VenueAnalyser _venueAnalyser;
        private readonly DocumentClient _documentClient;
        private readonly ILogger<FixApprenticeshipVenueReferences> _logger;
        private readonly Configuration _configuration;
        private readonly VenueCorrector _venueCorrector;

        /// <summary>
        /// Thread-safe cache of venues for a provider.
        /// See https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
        /// </summary>
        private readonly ConcurrentDictionary<int, Lazy<Task<IReadOnlyCollection<Venue>>>> _venueCache
            = new ConcurrentDictionary<int, Lazy<Task<IReadOnlyCollection<Venue>>>>();

        public FixApprenticeshipVenueReferences(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            VenueAnalyser venueAnalyser,
            DocumentClient documentClient,
            ILogger<FixApprenticeshipVenueReferences> logger,
            Configuration configuration,
            VenueCorrector venueCorrector)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _venueAnalyser = venueAnalyser ?? throw new ArgumentNullException(nameof(venueAnalyser));
            _documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _venueCorrector = venueCorrector ?? throw new ArgumentNullException(nameof(venueCorrector));
        }

        /// <summary>
        /// Analyse all the apprenticeships of a specific provider.
        /// Returns results as json and writes report to blob storage.
        /// For convenience when testing behaviour.
        /// </summary>
        [FunctionName(nameof(AnalyseProviderApprenticeshipVenueReferences))]
        public async Task<ApprenticeshipVenueCorrection[]> AnalyseProviderApprenticeshipVenueReferences(
            [HttpTrigger(methods: "post")] List<int> ukprns,
            [Blob(ReportBlobContainer, Connection=BlobConnectionStringKey)]CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            await using var reportBlobStream = await GetReportBlobStream(cloudBlobContainer);


            var apprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetApprenticeships {Predicate = a => ukprns.Contains(a.ProviderUKPRN)});

            var apprenticeshipVenueCorrections = new List<ApprenticeshipVenueCorrection>();
            foreach (var apprenticeship in apprenticeships.Values)
            {
                apprenticeshipVenueCorrections.Add(await Analyse(apprenticeship));
            }

            var counts = AnalysisCounts.GetCounts(apprenticeshipVenueCorrections);
            LogCounts(counts, "Specified providers' apprenticeships analysed.");

            await WriteReport(reportBlobStream, apprenticeshipVenueCorrections);

            return apprenticeshipVenueCorrections.ToArray();
        }

        /// <summary>
        /// Returns id of next apprenticeship that has an available fix, or null if none left to fix.
        /// For convenience when testing behaviour.
        /// </summary>
        [FunctionName(nameof(FindNextFixableCorruption))]
        public async Task<Guid?> FindNextFixableCorruption(
            [HttpTrigger(methods: "post")]
            string unusedWorkaroundParam, // Unused param is a work around for https://github.com/Azure/azure-functions-vs-build-sdk/issues/168
            [Blob(ReportBlobContainer, Connection = BlobConnectionStringKey)]
            CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                _configuration.DatabaseId,
                _configuration.ApprenticeshipCollectionName);

            var query = _documentClient.CreateDocumentQuery<Apprenticeship>(
                    collectionUri,
                    new FeedOptions {EnableCrossPartitionQuery = true, MaxItemCount = BatchSize})
                .Where(LiveApprenticeshipPredicate());

            var documentQuery = query.AsDocumentQuery();
            while (documentQuery.HasMoreResults)
            {
                var response = await documentQuery.ExecuteNextAsync<Apprenticeship>();
                foreach (var apprenticeship in response)
                {
                    _logger.LogInformation($"{LogPrefix} findnext fixable... {apprenticeship.Id}");
                    var analysis = await Analyse(apprenticeship);
                    if (analysis.ApprenticeshipLocationVenueCorrections.Any(c => c.VenueCorrection != null))
                    {
                        _logger.LogInformation($"{LogPrefix} fixable corrupt: {apprenticeship.Id}");
                        return analysis.Apprenticeship.Id;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns id of next apprenticeship that is considered to be corrupt (we might not be able to fix it),
        /// or null if none left to fix.
        /// For convenience when testing behaviour.
        /// </summary>
        [FunctionName(nameof(FindNextCorruption))]
        public async Task<Guid?> FindNextCorruption(
            [HttpTrigger(methods: "post")]
            string unusedWorkaroundParam, // Unused param is a work around for https://github.com/Azure/azure-functions-vs-build-sdk/issues/168
            [Blob(ReportBlobContainer, Connection = BlobConnectionStringKey)]
            CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                _configuration.DatabaseId,
                _configuration.ApprenticeshipCollectionName);

            var query = _documentClient.CreateDocumentQuery<Apprenticeship>(
                    collectionUri,
                    new FeedOptions {EnableCrossPartitionQuery = true, MaxItemCount = BatchSize})
                .Where(LiveApprenticeshipPredicate());

            var documentQuery = query.AsDocumentQuery();
            while (documentQuery.HasMoreResults)
            {
                var response = await documentQuery.ExecuteNextAsync<Apprenticeship>();
                foreach (var apprenticeship in response)
                {
                    // _logger.LogInformation($"{LogPrefix} findnext... {apprenticeship.Id}");
                    var analysis = await Analyse(apprenticeship);
                    if (analysis.ApprenticeshipLocationVenueCorrections.Any())
                    {
                        // _logger.LogInformation($"{LogPrefix} corrupt: {apprenticeship.Id}");
                        return analysis.Apprenticeship.Id;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Analyse apprenticeships specified by id in the request parameters.
        /// Returns results as json and writes report to blob storage.
        /// For convenience when testing behaviour.
        /// Usage:
        /// curl http://localhost:7071/api/AnalyseSpecificApprenticeshipVenueReferences -d '[3BD8BAF8-01EC-410C-9B67-6B839AF6BE1C,6DE16FF9-53FA-4218-97C2-FE95AC3681EC]' -H "Content-Type:application/json" | jq
        /// </summary>
        [FunctionName(nameof(AnalyseSpecificApprenticeshipVenueReferences))]
        public async Task<ApprenticeshipVenueCorrection[]> AnalyseSpecificApprenticeshipVenueReferences(
            [HttpTrigger(methods: "post")] List<Guid> apprenticeshipIds,
            [Blob(ReportBlobContainer, Connection=BlobConnectionStringKey)]CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            await using var reportBlobStream = await GetReportBlobStream(cloudBlobContainer);

            var apprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetApprenticeshipsByIds {ApprenticeshipIds = apprenticeshipIds});

            var apprenticeshipVenueCorrections = new List<ApprenticeshipVenueCorrection>();
            foreach (var apprenticeship in apprenticeships.Values)
            {
                apprenticeshipVenueCorrections.Add(await Analyse(apprenticeship));
            }

            var counts = AnalysisCounts.GetCounts(apprenticeshipVenueCorrections);
            LogCounts(counts, "Specified apprenticeships analysed.");

            await WriteReport(reportBlobStream, apprenticeshipVenueCorrections);

            return apprenticeshipVenueCorrections.ToArray();
        }

        /// <summary>
        /// Fix corruption in apprenticeships specified by id in the request parameters.
        /// Writes report to blob storage.
        /// For convenience when testing behaviour (we'll use the FixAll endpoint in production).
        /// We have this narrower fix endpoint because ones records have been corrected in a test environment
        /// there's nothing to fix on a subsequent test run.
        /// Usage:
        /// curl http://localhost:7071/api/FixSpecificApprenticeshipVenueReferences -d '[3BD8BAF8-01EC-410C-9B67-6B839AF6BE1C,6DE16FF9-53FA-4218-97C2-FE95AC3681EC]' -H "Content-Type:application/json" | jq
        /// </summary>
        [FunctionName(nameof(FixSpecificApprenticeshipVenueReferences))]
        public async Task FixSpecificApprenticeshipVenueReferences([HttpTrigger(methods:"post")]List<Guid> apprenticeshipIds,
            [Blob(ReportBlobContainer, Connection=BlobConnectionStringKey)]CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            await using var reportBlobStream = await GetReportBlobStream(cloudBlobContainer, fixup: true);

            var apprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetApprenticeshipsByIds{ApprenticeshipIds = apprenticeshipIds});

            var apprenticeshipVenueCorrections = new List<ApprenticeshipVenueCorrection>();
            foreach (var apprenticeship in apprenticeships.Values)
            {
                apprenticeshipVenueCorrections.Add(await Analyse(apprenticeship));
            }

            var counts = AnalysisCounts.GetCounts(apprenticeshipVenueCorrections);
            LogCounts(counts, "Batch analysed.");

            _logger.LogInformation($"{LogPrefix} Beginning fixup of specified apprenticeships...");
            try
            {
                foreach (var apprenticeshipVenueCorrection in apprenticeshipVenueCorrections)
                {
                    shutdownCancellationToken.ThrowIfCancellationRequested();
                    await ApplyCorrection(apprenticeshipVenueCorrection, shutdownCancellationToken);
                }
            }
            finally
            {
                await WriteReport(reportBlobStream, apprenticeshipVenueCorrections);
            }
            var failureTotal = apprenticeshipVenueCorrections.Count(c => c.UpdateFailure != null);
            if (failureTotal > 0)
            {
                _logger.LogError($"{LogPrefix} {failureTotal} FAILED UPDATES (see report in blob storage).");
            }
            _logger.LogInformation($"{LogPrefix} Completed fixup of specific apprenticeships.");
        }

        /// <summary>
        /// Analyse all apprenticeships.
        /// Writes report to blob storage.
        /// Intended for production use as a dry-run before running the final fix.
        /// Usage:
        /// curl http://localhost:7071/api/AnalyseAllApprenticeshipVenueReferences -d '{}' -H "Content-Type:application/json"
        /// </summary>
        /// <param name="shutdownCancellationToken">
        /// Note that the cancellation token is only for host shutdown not for aborted requests. https://stackoverflow.com/questions/60202259/azure-functions-using-cancellation-token-with-http-trigger/63439515#63439515
        /// </param>
        [FunctionName(nameof(AnalyseAllApprenticeshipVenueReferences))]
        public async Task AnalyseAllApprenticeshipVenueReferences(
            [HttpTrigger(methods: "post")]
            string unusedWorkaroundParam, // Unused param is a work around for https://github.com/Azure/azure-functions-vs-build-sdk/issues/168
            [Blob(ReportBlobContainer, Connection = BlobConnectionStringKey)]
            CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            await using var reportBlobStream = await GetReportBlobStream(cloudBlobContainer);
            var totals = new AnalysisCounts();
            int batchCounter = 0;

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllApprenticeships
                {
                    Predicate = LiveApprenticeshipPredicate(),
                    MaxBatchSize = BatchSize,
                    ProcessChunk = async apprenticeshipChunk =>
                    {
                        Interlocked.Increment(ref batchCounter);
                        var apprenticeshipVenueCorrections = new List<ApprenticeshipVenueCorrection>();
                        try
                        {
                            if (shutdownCancellationToken.IsCancellationRequested)
                            {
                                _logger.LogWarning($"{LogPrefix} Cancellation requested, flushing logs and exiting.");
                                shutdownCancellationToken.ThrowIfCancellationRequested();
                            }
                            foreach (var apprenticeship in apprenticeshipChunk)
                            {
                                apprenticeshipVenueCorrections.Add(await Analyse(apprenticeship));
                            }
                            var counts = AnalysisCounts.GetCounts(apprenticeshipVenueCorrections);
                            LogCounts(counts, $"Batch {batchCounter} analysed.");
                            totals = totals.Add(counts);
                        }
                        finally
                        {
                            await WriteReport(reportBlobStream, apprenticeshipVenueCorrections);
                        }
                    },
                });
            LogCounts(totals, "Grand total:");
            _logger.LogInformation($"{LogPrefix} Analysis of all data completed.");
        }

        /// <summary>
        /// Fix all corrupt apprenticeships that we can fix safely.
        /// Returns results as json and writes report to blob storage.
        /// Intended for production use as a dry-run before running the final fix.
        /// Usage:
        /// curl http://localhost:7071/api/FixAllApprenticeshipVenueReferences -d '{}' -H "Content-Type:application/json"
        /// </summary>
        /// <param name="shutdownCancellationToken">
        /// Note that the cancellation token is only for host shutdown not for aborted requests. https://stackoverflow.com/questions/60202259/azure-functions-using-cancellation-token-with-http-trigger/63439515#63439515
        /// </param>
        [FunctionName(nameof(FixAllApprenticeshipVenueReferences))]
        public async Task FixAllApprenticeshipVenueReferences(
            [HttpTrigger(methods: "post")]
            string unusedWorkaroundParam, // Unused param is a work around for https://github.com/Azure/azure-functions-vs-build-sdk/issues/168
            [Blob(ReportBlobContainer, Connection = BlobConnectionStringKey)]
            CloudBlobContainer cloudBlobContainer,
            CancellationToken shutdownCancellationToken)
        {
            await using var reportBlobStream = await GetReportBlobStream(cloudBlobContainer, fixup: true);
            var totals = new AnalysisCounts();
            int batchCounter = 0;
            int failureTotal = 0;

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllApprenticeships
                {
                    Predicate = LiveApprenticeshipPredicate(),
                    MaxBatchSize = BatchSize,
                    ProcessChunk = async apprenticeshipChunk =>
                    {
                        Interlocked.Increment(ref batchCounter);
                        var apprenticeshipVenueCorrections = new List<ApprenticeshipVenueCorrection>();
                        try
                        {
                            foreach (var apprenticeship in apprenticeshipChunk)
                            {
                                apprenticeshipVenueCorrections.Add(await Analyse(apprenticeship));
                            }
                            var counts = AnalysisCounts.GetCounts(apprenticeshipVenueCorrections);
                            LogCounts(counts, $"Batch {batchCounter} analysed for fixup.");
                            totals = totals.Add(counts);

                            _logger.LogInformation($"{LogPrefix} Applying batch fixes...");
                            foreach (var apprenticeshipVenueCorrection in apprenticeshipVenueCorrections)
                            {
                                if (shutdownCancellationToken.IsCancellationRequested)
                                {
                                    _logger.LogWarning($"{LogPrefix} Cancellation requested, flushing logs and exiting.");
                                    shutdownCancellationToken.ThrowIfCancellationRequested();
                                }
                                await ApplyCorrection(apprenticeshipVenueCorrection, shutdownCancellationToken);
                            }
                            failureTotal += apprenticeshipVenueCorrections.Count(c => c.UpdateFailure != null);
                        }
                        finally
                        {
                            await WriteReport(reportBlobStream, apprenticeshipVenueCorrections);
                        }
                    },
                });
            LogCounts(totals, "Fixup grand total:");
            if (failureTotal > 0)
            {
                _logger.LogError($"{LogPrefix} {failureTotal} FAILED UPDATES (see report in blob storage).");
            }
            _logger.LogInformation($"{LogPrefix} Venue fixup for all data completed.");
        }

        private static Expression<Func<Apprenticeship, bool>> LiveApprenticeshipPredicate()
        {
            // Filtering to match GetApprenticeshipsByIdsHandler behaviour
            return a => a.RecordStatus != (int)ApprenticeshipStatus.Archived &&
                        a.RecordStatus != (int)ApprenticeshipStatus.Deleted;
        }

        private async Task<ApprenticeshipVenueCorrection> Analyse(Apprenticeship apprenticeship)
        {
            // _logger.LogDebug($"{LogPrefix} Analysing apprenticeshipId {apprenticeship.Id} ...");

            // Thread-safe cache of venues for a provider.
            // See https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
            var providerVenues = await _venueCache.GetOrAdd(apprenticeship.ProviderUKPRN,
                new Lazy<Task<IReadOnlyCollection<Venue>>>(() => GetVenues(apprenticeship.ProviderUKPRN))).Value;

            var correction = _venueAnalyser.AnalyseApprenticeship(apprenticeship, providerVenues);

            // debug output useful for picking ones out to test fix-specific endpoints
            // var fixable = correction.ApprenticeshipLocationVenueCorrections.Count(l => l.VenueCorrection != null);
            // if (fixable > 0) _logger.LogDebug($"Apprenticeship {apprenticeship.Id}: {fixable} fixable locations");

            return correction;
        }

        private async Task<IReadOnlyCollection<Venue>> GetVenues(int ukprn)
        {
            try
            {
                var venues = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetVenuesByProvider {ProviderUkprn = ukprn});

                return venues.Where(v => v.Status == (int)VenueStatus.Live).ToList();
            }
            catch (Exception exception)
            {
                var message = $"Error fetching venues for ukprn {ukprn}. {exception.Message}"; // repeat exception in message because it's not showing in console output
                _logger.LogError($"{LogPrefix} {message}", exception);
                throw new Exception(message, exception);
            }
        }

        private async Task ApplyCorrection(ApprenticeshipVenueCorrection apprenticeshipVenueCorrection, CancellationToken cancellationToken)
        {
            if (!_venueCorrector.Apply(apprenticeshipVenueCorrection)) // mutates apprenticeship directly
            {
                return; // no changes to apply
            }

            var documentUri = UriFactory.CreateDocumentUri(
                _configuration.DatabaseId,
                _configuration.ApprenticeshipCollectionName,
                apprenticeshipVenueCorrection.Apprenticeship.Id.ToString());

            // Dropping to native cosmosdb libraries here rather than cqrs commands because
            // this gives us a complete round-trip without either:
            // * introducing another read operation which would be significant on 40k records in terms of time and RU limits, or
            // * blowing a hole in the command/query api by just exposing ReplaceDocument capability

            _logger.LogDebug($"{LogPrefix} Replacing apprenticeship document at uri {documentUri} with corrected copy...");
            try
            {
                var response = await _documentClient.ReplaceDocumentAsync(documentUri, apprenticeshipVenueCorrection.Apprenticeship,
                    new RequestOptions
                    {
                        AccessCondition = new AccessCondition
                        {
                            Type = AccessConditionType.IfMatch,
                            Condition = apprenticeshipVenueCorrection.Apprenticeship.ETag,
                        }
                    },
                    cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var failure = $"ReplaceDocument failed for {documentUri}, cosmos responded with status code: {response.StatusCode}";
                    apprenticeshipVenueCorrection.UpdateFailure = failure;
                    _logger.LogError($"{LogPrefix} {failure}");
                }
                else
                {
                    apprenticeshipVenueCorrection.UpdatedDate = DateTime.UtcNow;
                }
            }
            catch (Exception exception)
            {
                var message = $"{LogPrefix} Error replacing apprenticeship at {documentUri}. {exception.Message}";
                apprenticeshipVenueCorrection.UpdateFailure = message;
                _logger.LogError($"{LogPrefix} {message}", exception);
            }
        }

        private async Task<CloudBlobStream> GetReportBlobStream(CloudBlobContainer cloudBlobContainer, bool fixup = false)
        {
            await cloudBlobContainer.CreateIfNotExistsAsync();
            var blobName = $"VenueFixup_{DateTime.UtcNow:yyyyMMddTHHmmssZ}_{(fixup ? "fixup" : "analysis")}.json";
            _logger.LogInformation($"{LogPrefix} Streaming report to blob storage at '{cloudBlobContainer.Uri}/{blobName}'");
            var blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            return await blob.OpenWriteAsync();
        }

        private async Task WriteReport(Stream reportBlobStream, IEnumerable<ApprenticeshipVenueCorrection> apprenticeshipVenueCorrections)
        {
            // This method just appends arrays to the file, which isn't valid json but it's close enough for this job and ensures we have all the data if needed

            // No cancellation token passed through because we always want the reporting to finish
            await JsonSerializer.SerializeAsync(reportBlobStream, apprenticeshipVenueCorrections,
                new JsonSerializerOptions
                {
                    WriteIndented = true, // adds ~38% to file size, total analysis on dev is 100Mb with this which seems okay
                    Converters = {new JsonStringEnumConverter()}
                });
            await reportBlobStream.WriteAsync(Encoding.UTF8.GetBytes("\r\n")); // split the batches to make the massive json files a tiny bit more manageable
            await reportBlobStream.FlushAsync();
            _logger.LogDebug($"{LogPrefix} Report chunk written to blob.");
        }

        private void LogCounts(AnalysisCounts counts, string message)
        {
            var analysisStrings = counts.FixCounts.Select(c => c.UnfixableLocationVenueReason == null
                ? $"{c.CorruptionType}/Fixable {c.Count}"
                : $"{c.CorruptionType}/{c.UnfixableLocationVenueReason} {c.Count}");

            _logger.LogInformation(
                $"{LogPrefix} {message} Batch size: {counts.BatchSize}. Corrupt locations analysed: {counts.CorruptLocationsAnalysed}. ({string.Join("; ", analysisStrings)}).");
        }
    }
}
