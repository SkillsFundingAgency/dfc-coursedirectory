using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Extensions.Logging;
using Polly;

namespace Dfc.CourseDirectory.Core
{
    public class SqlDataSync
    {
        private const int BatchSize = 150;

        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public SqlDataSync(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock,
            ILogger<SqlDataSync> logger)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _logger = logger;
        }

        public async Task SyncAll()
        {
            await SyncAllProviders();
            await SyncAllVenues();
            await SyncAllCourses();
            await SyncAllApprenticeships();
        }

        public Task SyncAllApprenticeships() => WithExclusiveSqlLock(
            nameof(SyncAllApprenticeships),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllApprenticeships()
                {
                    ProcessChunk = GetSyncWithBatchingHandler<Apprenticeship>(SyncApprenticeships)
                }));
			
        public Task SyncAllCourses() => WithExclusiveSqlLock(
            nameof(SyncAllCourses),
            () =>_cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllCourses()
                {
                    ProcessChunk = GetSyncWithBatchingHandler<Course>(SyncCourses)
                }));

        public Task SyncAllProviders() => WithExclusiveSqlLock(
            nameof(SyncAllProviders),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllProviders()
                {
                    ProcessChunk = GetSyncWithBatchingHandler<Provider>(SyncProviders)
                }));

        public Task SyncAllVenues() => WithExclusiveSqlLock(
            nameof(SyncAllVenues),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllVenues()
                {
                    ProcessChunk = GetSyncWithBatchingHandler<Venue>(SyncVenues)
                }));

        public Task SyncApprenticeship(Apprenticeship apprenticeship) => SyncApprenticeships(new[] { apprenticeship });

        public Task SyncApprenticeships(IEnumerable<Apprenticeship> apprenticeships) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertApprenticeshipsFromCosmos()
            {
                Records = apprenticeships.Select(apprenticeship => new UpsertApprenticeshipRecord()
                {
                    ApprenticeshipId = apprenticeship.Id,
                    ApprenticeshipStatus = (ApprenticeshipStatus)apprenticeship.RecordStatus,
                    CreatedOn = apprenticeship.CreatedDate,
                    CreatedBy = apprenticeship.CreatedBy,
                    UpdatedOn = apprenticeship.UpdatedDate,
                    UpdatedBy = apprenticeship.UpdatedBy,
                    TribalApprenticeshipId = apprenticeship.ApprenticeshipId,
                    ProviderUkprn = apprenticeship.ProviderUKPRN,
                    ProviderId = apprenticeship.ProviderId,
                    ApprenticeshipType = apprenticeship.ApprenticeshipType,
                    ApprenticeshipTitle = apprenticeship.ApprenticeshipTitle,
                    StandardCode = apprenticeship.StandardCode,
                    StandardVersion = apprenticeship.Version,
                    FrameworkCode = apprenticeship.FrameworkCode,
                    FrameworkProgType = apprenticeship.ProgType,
                    FrameworkPathwayCode = apprenticeship.PathwayCode,
                    MarketingInformation = apprenticeship.MarketingInformation,
                    ApprenticeshipWebsite = apprenticeship.Url,
                    ContactTelephone = apprenticeship.ContactTelephone,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactWebsite = apprenticeship.ContactWebsite,
                    BulkUploadErrorCount = apprenticeship.BulkUploadErrors?.Count ?? 0,
                    Locations = apprenticeship.ApprenticeshipLocations.Select(location => new UpsertApprenticeshipRecordLocation()
                    {
                        ApprenticeshipLocationId = location.Id,
                        ApprenticeshipLocationStatus = (ApprenticeshipStatus)location.RecordStatus,
                        CreatedOn = location.CreatedDate,
                        CreatedBy = location.CreatedBy,
                        UpdatedOn = location.UpdatedDate,
                        UpdatedBy = location.UpdatedBy,
                        Telephone = location.Phone,
                        VenueId = location.VenueId,
                        TribalApprenticeshipLocationId = location.ApprenticeshipLocationId,
                        National = location.National,
                        Radius = location.Radius,
                        LocationType = location.LocationType,
                        ApprenticeshipLocationType = location.ApprenticeshipLocationType,
                        Name = location.Name,
                        DeliveryModes = location.DeliveryModes,
                        Regions = location.Regions ?? Array.Empty<string>(),
                        LocationGuidId = location.LocationGuidId
                    })
                }),
                LastSyncedFromCosmos = _clock.UtcNow
            }));
			
        public Task SyncCourse(Course course) => SyncCourses(new[] { course });

        public Task SyncCourses(IEnumerable<Course> courses) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertCoursesFromCosmos()
            {
                Records = courses.Select(course => new UpsertCoursesRecord()
                {
                    CourseId = course.Id,
                    CourseStatus = course.CourseStatus,
                    CreatedOn = course.CreatedDate,
                    CreatedBy = course.CreatedBy,
                    UpdatedOn = course.UpdatedDate,
                    UpdatedBy = course.UpdatedBy,
                    TribalCourseId = course.CourseId,
                    LearnAimRef = course.LearnAimRef,
                    ProviderUkprn = course.ProviderUKPRN,
                    CourseDescription = course.CourseDescription,
                    EntryRequirements = course.EntryRequirements,
                    WhatYoullLearn = course.WhatYoullLearn,
                    HowYoullLearn = course.HowYoullLearn,
                    WhatYoullNeed = course.WhatYoullNeed,
                    HowYoullBeAssessed = course.HowYoullBeAssessed,
                    WhereNext = course.WhereNext,
                    BulkUploadErrorCount = course.BulkUploadErrors?.Count() ?? 0,
                    CourseRuns = course.CourseRuns.Select(courseRun => new UpsertCoursesRecordCourseRun()
                    {
                        CourseRunId = courseRun.Id,
                        CourseRunStatus = courseRun.RecordStatus,
                        CreatedOn = courseRun.CreatedDate,
                        CreatedBy = courseRun.CreatedBy,
                        UpdatedOn = courseRun.UpdatedDate,
                        UpdatedBy = courseRun.UpdatedBy,
                        CourseName = courseRun.CourseName,
                        VenueId = courseRun.VenueId,
                        ProviderCourseId = courseRun.ProviderCourseID,
                        DeliveryMode = courseRun.DeliveryMode,
                        FlexibleStartDate = courseRun.FlexibleStartDate,
                        StartDate = courseRun.StartDate,
                        CourseWebsite = courseRun.CourseURL,
                        Cost = courseRun.Cost,
                        CostDescription = courseRun.CostDescription,
                        DurationUnit = courseRun.DurationUnit,
                        DurationValue = courseRun.DurationValue,
                        StudyMode = courseRun.StudyMode,
                        AttendancePattern = courseRun.AttendancePattern,
                        National = courseRun.National,
                        RegionIds = courseRun.Regions ?? Array.Empty<string>(),
                        SubRegionIds = courseRun.SubRegions?.Select(r => r.Id) ?? Array.Empty<string>(),
                        BulkUploadErrorCount = courseRun.BulkUploadErrors?.Count() ?? 0
                    })
                }),
                LastSyncedFromCosmos = _clock.UtcNow
            }));

        public Task SyncProvider(Provider provider) => SyncProviders(new[] { provider });

        public Task SyncProviders(IEnumerable<Provider> providers) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertProvidersFromCosmos()
            {
                Records = providers.Select(provider => new UpsertProvidersRecord()
                {
                    ProviderId = provider.Id,
                    Ukprn = provider.Ukprn,
                    ProviderStatus = provider.Status,
                    ProviderType = provider.ProviderType,
                    ProviderName = provider.ProviderName,
                    UkrlpProviderStatusDescription = provider.ProviderStatus,
                    MarketingInformation = provider.MarketingInformation,
                    CourseDirectoryName = provider.CourseDirectoryName,
                    TradingName = provider.TradingName,
                    Alias = provider.Alias,
                    UpdatedOn = provider.DateUpdated != default ? (DateTime?)provider.DateUpdated : null,
                    UpdatedBy = provider.UpdatedBy,
                    NationalApprenticeshipProvider = provider.NationalApprenticeshipProvider,
                    TribalProviderId = provider.ProviderId,
                    BulkUploadInProgress = provider.BulkUploadStatus?.InProgress,
                    BulkUploadPublishInProgress = provider.BulkUploadStatus?.PublishInProgress,
                    BulkUploadStartedDateTime = provider.BulkUploadStatus?.StartedTimestamp,
                    BulkUploadTotalRowCount = provider.BulkUploadStatus?.TotalRowCount,
                    Contacts = (provider.ProviderContact ?? Array.Empty<ProviderContact>()).Select(c => new UpsertProvidersRecordContact()
                    {
                        ContactType = c.ContactType,
                        ContactRole = c.ContactRole,
                        AddressSaonDescription = c.ContactAddress?.SAON?.Description,
                        AddressPaonDescription = c.ContactAddress?.PAON?.Description,
                        AddressStreetDescription = c.ContactAddress?.StreetDescription,
                        AddressLocality = c.ContactAddress?.Locality,
                        AddressItems = string.Join(" ", c.ContactAddress?.Items ?? Array.Empty<string>()),
                        AddressPostTown = c.ContactAddress?.PostTown ?? c.ContactAddress?.Items.ElementAtOrDefault(0),
                        AddressCounty = c.ContactAddress?.County ?? c.ContactAddress?.Items.ElementAtOrDefault(1),
                        AddressPostcode = c.ContactAddress?.PostCode,
                        PersonalDetailsPersonNameTitle = string.Join(" ", c.ContactPersonalDetails?.PersonNameTitle ?? Array.Empty<string>()),
                        PersonalDetailsPersonNameGivenName = string.Join(" ", c.ContactPersonalDetails?.PersonGivenName ?? Array.Empty<string>()),
                        PersonalDetailsPersonNameFamilyName = c.ContactPersonalDetails?.PersonFamilyName,
                        Telephone1 = c.ContactTelephone1,
                        Telephone2 = c.ContactTelephone2,
                        Fax = c.ContactFax,
                        WebsiteAddress = c.ContactWebsiteAddress,
                        Email = c.ContactEmail
                    })
                }),
                LastSyncedFromCosmos = _clock.UtcNow
            }));

        public Task SyncVenue(Venue venue) => SyncVenues(new[] { venue });

        public Task SyncVenues(IEnumerable<Venue> venues) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertVenuesFromCosmos()
            {
                Records = venues.Select(venue => new UpsertVenuesRecord()
                {
                    VenueId = venue.Id,
                    VenueStatus = (VenueStatus)venue.Status,
                    CreatedOn = venue.CreatedDate != default ? (DateTime?)venue.CreatedDate : null,
                    CreatedBy = venue.CreatedBy,
                    UpdatedOn = venue.DateUpdated != default ? (DateTime?)venue.DateUpdated : null,
                    UpdatedBy = venue.UpdatedBy,
                    VenueName = venue.VenueName,
                    ProviderUkprn = venue.Ukprn,
                    TribalVenueId = venue.LocationId,
                    ProviderVenueRef = venue.ProvVenueID,
                    AddressLine1 = venue.AddressLine1,
                    AddressLine2 = venue.AddressLine2,
                    Town = venue.Town,
                    County = venue.County,
                    Postcode = venue.Postcode,
                    Position = ((double)venue.Latitude, (double)venue.Longitude),
                    Telephone = venue.PHONE,
                    Email = venue.Email,
                    Website = venue.Website
                }),
                LastSyncedFromCosmos = _clock.UtcNow
            }));

        private static Func<IReadOnlyCollection<T>, Task> GetSyncWithBatchingHandler<T>(
            Func<IReadOnlyCollection<T>, Task> processChunk) => async records =>
            {
                foreach (IReadOnlyCollection<T> c in records.Buffer(BatchSize))
                {
                    await Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(retry))
                        .ExecuteAsync(() => processChunk(c));
                }
            };

        private async Task WithExclusiveSqlLock(string lockName, Func<Task> action)
        {
            // Grab an exclusive lock inside a transaction that spans the entire duration of `action`'s execution.
            // ISqlQueryDispatcher owns the transaction; Dispose()ing the scope Dispose()s the transaction too.
            // Note that commiting this transaction is not necessary.
            // If the lock cannot be grabbed immediately then log & bail.

            var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var result = await sqlDispatcher.ExecuteQuery(new GetExclusiveLock()
            {
                Name = lockName,
                TimeoutMilliseconds = 0  // Return immediately if lock cannot be acquired
            });

            if (!result)
            {
                _logger.LogWarning($"Failed to acquire exclusive lock: '{lockName}'.");
                return;
            }

            await action();
        }

        private async Task WithSqlDispatcher(Func<ISqlQueryDispatcher, Task> action)
        {
            using (var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await action(sqlDispatcher);
                await sqlDispatcher.Commit();
            }
        }
    }
}
