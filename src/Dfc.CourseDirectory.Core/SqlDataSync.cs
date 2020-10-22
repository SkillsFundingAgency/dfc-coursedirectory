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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Dfc.CourseDirectory.Core
{
    public class SqlDataSync
    {
        private const int ApprenticeshipBatchSize = 300;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ILogger _logger;

        public SqlDataSync(
            IServiceScopeFactory serviceScopeFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ILogger<SqlDataSync> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
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
                    ProcessChunk = async chunk =>
                    {
                        foreach (var c in chunk.Buffer(ApprenticeshipBatchSize))
                        {
                            await Policy
                                .Handle<SqlException>()
                                .Retry(3)
                                .Execute(() => SyncApprenticeships(c));
                        }
                    }
                }));
			
        public Task SyncAllCourses() => WithExclusiveSqlLock(
            nameof(SyncAllCourses),
            () =>_cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllCourses()
                {
                    ProcessChunk = SyncCourses
                }));

        public Task SyncAllProviders() => WithExclusiveSqlLock(
            nameof(SyncAllProviders),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllProviders()
                {
                    ProcessChunk = SyncProviders
                }));

        public Task SyncAllVenues() => WithExclusiveSqlLock(
            nameof(SyncAllVenues),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllVenues()
                {
                    ProcessChunk = SyncVenues
                }));

        public Task SyncApprenticeship(Apprenticeship apprenticeship) => SyncApprenticeships(new[] { apprenticeship });

        public Task SyncApprenticeships(IEnumerable<Apprenticeship> apprenticeships) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertApprenticeships()
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
                })
            }));
			
        public Task SyncCourse(Course course) => SyncCourses(new[] { course });

        public Task SyncCourses(IEnumerable<Course> courses) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertCourses()
            {
                Records = courses.Select(course => new UpsertCoursesRecord()
                {
                    CourseId = course.Id,
                    CourseStatus = (CourseStatus)course.CourseStatus,
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
                    CourseRuns = course.CourseRuns.Select(courseRun => new UpsertCoursesRecordCourseRun()
                    {
                        CourseRunId = courseRun.Id,
                        CourseRunStatus = (CourseStatus)courseRun.RecordStatus,
                        CreatedOn = courseRun.CreatedDate,
                        CreatedBy = courseRun.CreatedBy,
                        UpdatedOn = courseRun.UpdatedDate,
                        UpdatedBy = courseRun.UpdatedBy,
                        CourseName = courseRun.CourseName,
                        VenueId = courseRun.VenueId,
                        ProviderCourseId = courseRun.ProviderCourseID,
                        DeliveryMode = (CourseDeliveryMode)courseRun.DeliveryMode,
                        FlexibleStartDate = courseRun.FlexibleStartDate,
                        StartDate = courseRun.StartDate,
                        CourseWebsite = courseRun.CourseURL,
                        Cost = courseRun.Cost,
                        CostDescription = courseRun.CostDescription,
                        DurationUnit = (CourseDurationUnit)courseRun.DurationUnit,
                        DurationValue = courseRun.DurationValue,
                        StudyMode = (CourseStudyMode)courseRun.StudyMode,
                        AttendancePattern = (CourseAttendancePattern)courseRun.AttendancePattern,
                        National = courseRun.National,
                        Regions = courseRun.Regions ?? Array.Empty<string>()
                    })
                })
            }));

        public Task SyncProvider(Provider provider) => SyncProviders(new[] { provider });

        public Task SyncProviders(IEnumerable<Provider> providers) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertProviders()
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
                    Contacts = (provider.ProviderContact ?? Array.Empty<ProviderContact>()).Select(c => new UpsertProvidersRecordContact()
                    {
                        ContactType = c.ContactType,
                        ContactRole = c.ContactRole,
                        AddressSaonDescription = c.ContactAddress?.SAON?.Description,
                        AddressPaonDescription = c.ContactAddress?.PAON?.Description,
                        AddressStreetDescription = c.ContactAddress?.StreetDescription,
                        AddressLocality = c.ContactAddress?.Locality,
                        AddressItems = string.Join(" ", c.ContactAddress?.Items ?? Array.Empty<string>()),
                        AddressPostTown = c.ContactAddress?.PostTown,
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
                })
            }));

        public Task SyncVenue(Venue venue) => SyncVenues(new[] { venue });

        public Task SyncVenues(IEnumerable<Venue> venues) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertVenues()
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
                })
            }));

        private async Task WithExclusiveSqlLock(string lockName, Func<Task> action)
        {
            // Grab an exclusive lock inside a transaction that spans the entire duration of `action`'s execution.
            // ISqlQueryDispatcher owns the transaction; Dispose()ing the scope Dispose()s the transaction too.
            // Note that commiting this transaction is not necessary.
            // If the lock cannot be grabbed immediately then log & bail.

            using var scope = _serviceScopeFactory.CreateScope();

            var sqlDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

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
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var sqlDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                await action(sqlDispatcher);

                sqlDispatcher.Transaction.Commit();
            }
        }
    }
}
