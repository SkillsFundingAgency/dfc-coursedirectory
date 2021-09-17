﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;
using Dfc.ProviderPortal.Packages;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Services
{
    public class ApprenticeshipService : IApprenticeshipService
    {
        private readonly IDASHelper _DASHelper;
        private readonly IProviderServiceClient _providerServiceClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public ApprenticeshipService(
            IDASHelper DASHelper,
            IProviderServiceClient providerServiceClient,
            TelemetryClient telemetryClient,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _DASHelper = DASHelper ?? throw new ArgumentNullException(nameof(DASHelper));
            _providerServiceClient = providerServiceClient ?? throw new ArgumentNullException(nameof(providerServiceClient));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        public async Task<IEnumerable<Apprenticeship>> GetLiveApprenticeships()
        {
            var apprenticeships = await _sqlQueryDispatcher.ExecuteQuery(new GetAllApprenticeships());

            return apprenticeships;
        }

        public async Task<IEnumerable<Apprenticeship>> GetApprenticeshipsByUkprn(int ukprn)
        {
            Throw.IfNull(ukprn, nameof(ukprn));
            Throw.IfLessThan(0, ukprn, nameof(ukprn));

            var provider = await _sqlQueryDispatcher.ExecuteQuery(new Core.DataStore.Sql.Queries.GetProviderByUkprn() { Ukprn = ukprn });

            if (provider == null)
            {
                return Enumerable.Empty<Apprenticeship>();
            }

            return await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipsForProvider() { ProviderId = provider.ProviderId });
        }

        /// <summary>
        /// Maps apprenticeships to provider(s) ready for export to DAS
        /// </summary>
        /// <param name="apprenticeships">A list of apprenticeships to be processed and grouped into Providers</param>
        /// <returns></returns>
        [Obsolete("This shouldn't be used any more - if possible replace with a mapping class using something like AutoMapper ", false)]
        public async Task<IEnumerable<DasProviderResult>> ApprenticeshipsToDasProviders(List<Apprenticeship> apprenticeships)
        {
            try
            {
                var timer = Stopwatch.StartNew();
                var evt = new EventTelemetry { Name = "ApprenticeshipsToDasProviders" };

                var apprenticeshipsByUKPRN = apprenticeships
                    .GroupBy(a => a.ProviderUkprn)
                    .OrderBy(g => g.Key)
                    .ToArray();

                var providers = (await _providerServiceClient.GetAllProviders())
                    .ToArray();

                var feChoices = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetFeChoicesByProviderUkprns { ProviderUkprns = apprenticeshipsByUKPRN.Select(a => a.Key) });

                evt.Metrics.TryAdd("Apprenticeships", apprenticeships.Count);
                evt.Metrics.TryAdd("Providers", apprenticeshipsByUKPRN.Length);

                Console.WriteLine($"[{DateTime.UtcNow:G}] Found {apprenticeships.Count} apprenticeships for {apprenticeshipsByUKPRN.Length} Providers");

                var results = new ConcurrentBag<DasProviderResult>();

                Parallel.ForEach(apprenticeshipsByUKPRN.Select((g, i) =>
                    new { UKPRN = g.Key, Index = i, Apprenticeships = g.ToList() }), p =>
                {
                    try
                    {
                        var provider = ExportProvider(
                            p.UKPRN,
                            p.Index + 1000,
                            providers.Where(pp => pp.UnitedKingdomProviderReferenceNumber == p.UKPRN.ToString()),
                            p.Apprenticeships,
                            feChoices.GetValueOrDefault(p.UKPRN));

                        results.Add(DasProviderResult.Succeeded(p.UKPRN, provider));

                        Console.WriteLine($"[{DateTime.UtcNow:G}][INFO] Exported {p.UKPRN} ({p.Index} of {p.Apprenticeships.Count})");
                    }
                    catch (ExportException ex)
                    {
                        results.Add(DasProviderResult.Failed(p.UKPRN, ex));

                        _telemetryClient.TrackException(ex);
                        Console.WriteLine($"[{DateTime.UtcNow:G}][ERROR] Failed to export {p.UKPRN} ({p.Index} of {p.Apprenticeships.Count})");
                    }
                });

                timer.Stop();

                var success = results.Count(r => r.Success);
                var failure = results.Count(r => !r.Success);

                Console.WriteLine($"[{DateTime.UtcNow:G}] Exported {results.Count(r => r.Success)} Providers in {timer.Elapsed.TotalSeconds} seconds.");

                if (failure > 0)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:G}] [WARNING] Encountered {failure} errors that need attention");
                }

                evt.Metrics.TryAdd("Export elapsed time (ms)", timer.ElapsedMilliseconds);
                evt.Metrics.TryAdd("Export success", success);
                evt.Metrics.TryAdd("Export failures", failure);
                _telemetryClient.TrackEvent(evt);

                return results.OrderBy(r => r.UKPRN).ToList();
            }
            catch (Exception e)
            {
                throw new ProviderServiceException(e);
            }
        }

        private DasProvider ExportProvider(
            int ukprn,
            int exportKey,
            IEnumerable<Models.Providers.Provider> providers,
            List<Apprenticeship> apprenticeships,
            Core.DataStore.CosmosDb.Models.FeChoice feChoice)
        {
            var evt = new EventTelemetry {Name = "ComposeProviderForExport"};

            evt.Properties.TryAdd("UKPRN", $"{ukprn}");
            evt.Metrics.TryAdd("ProviderApprenticeships", apprenticeships.Count());
            evt.Metrics.TryAdd("MatchingProviders", providers?.Count() ?? 0);

            if (!(providers?.Any() ?? false))
            {
                throw new ProviderNotFoundException(ukprn);
            }

            try
            {
                var dasProvider = _DASHelper.CreateDasProviderFromProvider(exportKey, providers.First(), feChoice);

                if (dasProvider != null)
                {
                    var apprenticeshipLocations = apprenticeships
                        .SelectMany(x => x.ApprenticeshipLocations).Distinct(new ApprenticeshipLocationSameAddress());

                    var index = 1000;
                    var locationIndex = new Dictionary<string, ApprenticeshipLocation>(apprenticeshipLocations
                        .Select(x => new KeyValuePair<string, ApprenticeshipLocation>($"{exportKey:D4}{index++:D4}", x)));

                    var exportStandards = apprenticeships;

                    evt.Metrics.TryAdd("Provider Locations", locationIndex.Count());
                    evt.Metrics.TryAdd("Provider Standards", exportStandards.Count());

                    dasProvider.Locations = _DASHelper.ApprenticeshipLocationsToLocations(exportKey, locationIndex);
                    dasProvider.Standards = _DASHelper.ApprenticeshipsToStandards(exportKey, exportStandards, locationIndex);
                    dasProvider.Frameworks = new List<DasFramework>();

                    _telemetryClient.TrackEvent(evt);

                    return dasProvider;
                }
            }
            catch (Exception e)
            {
                throw new ProviderExportException(ukprn, e);
            }

            throw new ProviderNotFoundException(ukprn);
        }

        internal IEnumerable<Apprenticeship> OnlyUpdatedCourses(IEnumerable<Apprenticeship> apprenticeships)
        {
            DateTime dateToCheckAgainst = DateTime.Now.Subtract(TimeSpan.FromDays(1));

            return apprenticeships.Where(x => x.UpdatedOn.HasValue && x.UpdatedOn> dateToCheckAgainst ||
                                       x.CreatedOn > dateToCheckAgainst).ToList();
        }
    }
}
