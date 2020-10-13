using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    public class SqlDataSyncTests : DatabaseTestBase
    {
        public SqlDataSyncTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task SyncProvider_UpsertsProvider()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = Guid.NewGuid(),
                UnitedKingdomProviderReferenceNumber = "12345",
                Status = Models.ProviderStatus.Onboarded,
                ProviderType = Models.ProviderType.Both,
                ProviderName = "Test Provider",
                ProviderStatus = "Active",
                MarketingInformation = "Marketing information",
                CourseDirectoryName = "Another name",
                TradingName = "Trading name",
                Alias = "Alias",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests",
                ProviderContact = new[]
                {
                    new ProviderContact()
                    {
                        ContactType = "P",
                        ContactRole = "Hero",
                        ContactPersonalDetails = new ProviderContactPersonalDetails()
                        {
                            PersonNameTitle = new[] { "Mr" },
                            PersonGivenName = new[] { "Person" },
                            PersonFamilyName = "Smith"
                        },
                        ContactAddress = new ProviderContactAddress()
                        {
                            SAON = new ProviderContactAddressSAON() { Description = "SAON" },
                            PAON = new ProviderContactAddressPAON() { Description = "PAON" },
                            StreetDescription = "Street",
                            Locality = "Locality",
                            Items = new[] { "Item1", "Item2" },
                            PostTown = "Town",
                            PostCode = "AB1 2CD"
                        },
                        ContactTelephone1 = "01234 567890",
                        ContactFax = "02345 678901",
                        ContactWebsiteAddress = "https://provider.com/contact",
                        ContactEmail = "person@provider.com",
                        LastUpdated = Clock.UtcNow
                    }
                }
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncProvider(provider);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertProviders, None>(q =>
                q.Records.Any(p =>
                    p.ProviderId == provider.Id &&
                    p.Ukprn == provider.Ukprn &&
                    p.ProviderStatus == Models.ProviderStatus.Onboarded &&
                    p.ProviderType == Models.ProviderType.Both &&
                    p.UkrlpProviderStatusDescription == "Active" &&
                    p.MarketingInformation == "Marketing information" &&
                    p.CourseDirectoryName == "Another name" &&
                    p.TradingName == "Trading name" &&
                    p.Alias == "Alias" &&
                    p.UpdatedOn == Clock.UtcNow &&
                    p.UpdatedBy == "Tests"));
        }

        [Fact]
        public async Task SyncVenue_UpsertsVenue()
        {
            // Arrange
            var venue = new Venue()
            {
                Id = Guid.NewGuid(),
                Ukprn = 12345,
                VenueName = "Test",
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Town = "Town",
                County = "County",
                Postcode = "AB1 2DE",
                Latitude = 1,
                Longitude = 2,
                PHONE = "01234 567890",
                Email = "venue@provider.com",
                Website = "https://provider.com/venue",
                Status = 1,
                LocationId = 42,
                ProvVenueID = "MY VENUE",
                CreatedDate = Clock.UtcNow,
                CreatedBy = "Tests",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests"
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncVenue(venue);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertVenues, None>(q =>
                q.Records.Any(v =>
                    v.VenueId == venue.Id &&
                    v.ProviderUkprn == 12345 &&
                    v.VenueName == "Test" &&
                    v.AddressLine1 == "Line 1" &&
                    v.AddressLine2 == "Line 2" &&
                    v.Town == "Town" &&
                    v.County == "County" &&
                    v.Postcode == "AB1 2DE" &&
                    v.Position.Latitude == 1 &&
                    v.Position.Longitude == 2 &&
                    v.Telephone == "01234 567890" &&
                    v.Email == "venue@provider.com" &&
                    v.Website == "https://provider.com/venue" &&
                    v.VenueStatus == Models.VenueStatus.Live &&
                    v.TribalVenueId == 42 &&
                    v.ProviderVenueRef == "MY VENUE" &&
                    v.CreatedBy == "Tests" &&
                    v.CreatedOn == Clock.UtcNow &&
                    v.UpdatedBy == "Tests" &&
                    v.UpdatedOn == Clock.UtcNow));
        }

        [Fact]
        public async Task SyncCourse_UpsertsCourse()
        {
            // Arrange
            var course = new Course()
            {
                Id = Guid.NewGuid(),
                ProviderId = Guid.NewGuid(),
                ProviderUKPRN = 12345,
                CourseId = 67890,
                QualificationCourseTitle = "Maths",
                LearnAimRef = "10101011",
                NotionalNVQLevelv2 = "3",
                AwardOrgCode = "TST",
                QualificationType = "Other",
                CourseDescription = "Maths description",
                EntryRequirements = "Entry requirements",
                WhatYoullLearn = "What you'll learn",
                HowYoullLearn = "How you'll learn",
                WhatYoullNeed = "What you'll need",
                HowYoullBeAssessed = "How you'll be assessed",
                WhereNext = "Where next",
                AdultEducationBudget = true,
                AdvancedLearnerLoan = true,
                CourseRuns = new[]
                {
                    new CourseRun()
                    {
                        Id = Guid.NewGuid(),
                        CourseInstanceId = 7890,
                        VenueId = Guid.NewGuid(),
                        CourseName = "Maths",
                        ProviderCourseID = "MATHS",
                        DeliveryMode = CourseDeliveryMode.ClassroomBased,
                        FlexibleStartDate = false,
                        StartDate = new DateTime(2020, 4, 1),
                        CourseURL = "https://provider.com/maths",
                        Cost = 3,
                        CostDescription = "£3",
                        DurationUnit = CourseDurationUnit.Months,
                        DurationValue = 6,
                        StudyMode = CourseStudyMode.PartTime,
                        AttendancePattern = CourseAttendancePattern.Evening,
                        National = true,
                        Regions = new[] { "E12000001" },  // North East
                        RecordStatus = CourseStatus.Live,
                        CreatedDate = Clock.UtcNow,
                        CreatedBy = "Tests",
                        UpdatedDate = Clock.UtcNow,
                        UpdatedBy = "Tests"
                    }
                },
                CourseStatus = CourseStatus.Live,
                CreatedDate = Clock.UtcNow,
                CreatedBy = "Tests",
                UpdatedDate = Clock.UtcNow,
                UpdatedBy = "Tests"
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncCourse(course);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertCourses, None>(q =>
            {
                var record = q.Records.SingleOrDefault();
                if (record == default)
                {
                    return false;
                }

                var recordCourseRun = record.CourseRuns.SingleOrDefault();
                if (recordCourseRun == default)
                {
                    return false;
                }

                return record.CourseId == course.Id &&
                    record.ProviderUkprn == 12345 &&
                    record.TribalCourseId == 67890 &&
                    record.LearnAimRef == "10101011" &&
                    record.CourseDescription == "Maths description" &&
                    record.EntryRequirements == "Entry requirements" &&
                    record.WhatYoullLearn == "What you'll learn" &&
                    record.HowYoullLearn == "How you'll learn" &&
                    record.WhatYoullNeed == "What you'll need" &&
                    record.HowYoullBeAssessed == "How you'll be assessed" &&
                    record.WhereNext == "Where next" &&
                    recordCourseRun.CourseRunId == course.CourseRuns.Single().Id &&
                    recordCourseRun.VenueId == course.CourseRuns.Single().VenueId &&
                    recordCourseRun.CourseName == "Maths" &&
                    recordCourseRun.ProviderCourseId == "MATHS" &&
                    recordCourseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased &&
                    recordCourseRun.FlexibleStartDate == false &&
                    recordCourseRun.StartDate == new DateTime(2020, 4, 1) &&
                    recordCourseRun.CourseWebsite == "https://provider.com/maths" &&
                    recordCourseRun.Cost == 3 &&
                    recordCourseRun.CostDescription == "£3" &&
                    recordCourseRun.DurationUnit == CourseDurationUnit.Months &&
                    recordCourseRun.DurationValue == 6 &&
                    recordCourseRun.StudyMode == CourseStudyMode.PartTime &&
                    recordCourseRun.AttendancePattern == CourseAttendancePattern.Evening &&
                    recordCourseRun.National == true &&
                    recordCourseRun.Regions.Single() == "E12000001" &&
                    recordCourseRun.CourseRunStatus == CourseStatus.Live &&
                    recordCourseRun.CreatedOn == Clock.UtcNow &&
                    recordCourseRun.CreatedBy == "Tests" &&
                    recordCourseRun.UpdatedOn == Clock.UtcNow &&
                    recordCourseRun.UpdatedBy == "Tests";
            });
        }

        [Fact]
        public async Task SyncApprenticeship_UpsertsApprenticeship()
        {
            // Arrange
            var apprenticeship = new Apprenticeship()
            {
                Id = Guid.NewGuid(),
                ProviderId = Guid.NewGuid(),
                ProviderUKPRN = 12345,
                ApprenticeshipTitle = "Test Apprenticeship",
                ApprenticeshipType = ApprenticeshipType.StandardCode,
                StandardId = Guid.NewGuid(),
                StandardCode = 123,
                Version = 2,
                NotionalNVQLevelv2 = "3",
                MarketingInformation = "Our amazing apprenticeship",
                Url = "https://provider.com/apprenticeship",
                ContactTelephone = "01234 567890",
                ContactEmail = "apprenticeship@provider.com",
                ContactWebsite = "https://provider.com",
                ApprenticeshipLocations = new List<ApprenticeshipLocation>()
                {
                    new ApprenticeshipLocation()
                    {
                        Id = Guid.NewGuid(),
                        VenueId = Guid.NewGuid(),
                        National = false,
                        DeliveryModes = new List<ApprenticeshipDeliveryMode>()
                        {
                            ApprenticeshipDeliveryMode.EmployerAddress,
                            ApprenticeshipDeliveryMode.DayRelease
                        },
                        Name = "The Place",
                        Phone = "01234 567890",
                        ProviderUKPRN = 12345,
                        Regions = new[] { "E12000001" },  // North East
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                        LocationType = LocationType.Venue,
                        Radius = 30,
                        RecordStatus = 1,
                        CreatedDate = Clock.UtcNow,
                        CreatedBy = "Tests",
                        UpdatedDate = Clock.UtcNow,
                        UpdatedBy = "Tests",
                        ApprenticeshipLocationId = 689
                    }
                },
                RecordStatus = 1,
                CreatedDate = Clock.UtcNow,
                CreatedBy = "Tests",
                UpdatedDate = Clock.UtcNow,
                UpdatedBy = "Tests",
                ApprenticeshipId = 56789
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncApprenticeship(apprenticeship);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertApprenticeships, None>(q =>
            {
                var record = q.Records.SingleOrDefault();
                if (record == default)
                {
                    return false;
                }

                var recordLocation = record.Locations.SingleOrDefault();
                if (recordLocation == default)
                {
                    return false;
                }

                return record.ApprenticeshipId == apprenticeship.Id &&
                    record.ProviderId == apprenticeship.ProviderId &&
                    record.ProviderUkprn == 12345 &&
                    record.ApprenticeshipTitle == "Test Apprenticeship" &&
                    record.ApprenticeshipType == ApprenticeshipType.StandardCode &&
                    record.StandardCode == 123 &&
                    record.StandardVersion == 2 &&
                    record.MarketingInformation == "Our amazing apprenticeship" &&
                    record.ApprenticeshipWebsite == "https://provider.com/apprenticeship" &&
                    record.ContactTelephone == "01234 567890" &&
                    record.ContactEmail == "apprenticeship@provider.com" &&
                    record.ContactWebsite == "https://provider.com" &&
                    recordLocation.ApprenticeshipLocationId == apprenticeship.ApprenticeshipLocations.Single().Id &&
                    recordLocation.VenueId == apprenticeship.ApprenticeshipLocations.Single().VenueId &&
                    recordLocation.National == false &&
                    recordLocation.DeliveryModes.Count() == 2 &&
                    recordLocation.DeliveryModes.Contains(ApprenticeshipDeliveryMode.EmployerAddress) &&
                    recordLocation.DeliveryModes.Contains(ApprenticeshipDeliveryMode.DayRelease) &&
                    recordLocation.Name == "The Place" &&
                    recordLocation.Telephone == "01234 567890" &&
                    recordLocation.Regions.Single() == "E12000001" &&
                    recordLocation.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased &&
                    recordLocation.LocationType == LocationType.Venue &&
                    recordLocation.Radius == 30 &&
                    recordLocation.TribalApprenticeshipLocationId == 689;
            });
        }
    }
}
