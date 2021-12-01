using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
{
    public class DownloadTests : MvcTestBase
    {
        public DownloadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_DownloadsValidFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");

            var standard = await TestData.CreateStandard();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), providerVenueRef: "VENUE_REF");

            var apprenticeship = await TestData.CreateApprenticeship(
                providerId: provider.ProviderId,
                standard: standard,
                createdBy: User.ToUserInfo(),
                locations: new[]
                {
                    new CreateApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress },
                        National = false,
                        SubRegionIds = new[] { "E06000001" }  // County Durham
                    },
                    new CreateApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.DayRelease, ApprenticeshipDeliveryMode.BlockRelease },
                        VenueId = venue.VenueId,
                        Radius = 25
                    },
                    new CreateApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.DayRelease, ApprenticeshipDeliveryMode.BlockRelease, ApprenticeshipDeliveryMode.EmployerAddress },
                        VenueId = venue.VenueId,
                        Radius = 50
                    }
                });

            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/apprenticeships/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_apprenticeships_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.Reader.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "STANDARD_CODE",
                "STANDARD_VERSION",
                "APPRENTICESHIP_INFORMATION",
                "APPRENTICESHIP_WEBPAGE",
                "CONTACT_EMAIL",
                "CONTACT_PHONE",
                "CONTACT_URL",
                "DELIVERY_METHOD",
                "VENUE",
                "YOUR_VENUE_REFERENCE",
                "RADIUS",
                "DELIVERY_MODE",
                "NATIONAL_DELIVERY",
                "SUB_REGION"
            });

            var rows = csvReader.GetRecords<CsvApprenticeshipRow>();
            rows.Should().BeEquivalentTo(new[]
            {
                new CsvApprenticeshipRow()
                {
                    StandardCode = standard.StandardCode.ToString(),
                    StandardVersion = standard.Version.ToString(),
                    ApprenticeshipInformation = apprenticeship.MarketingInformation,
                    ApprenticeshipWebpage = apprenticeship.ApprenticeshipWebsite,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactPhone = apprenticeship.ContactTelephone,
                    ContactUrl = apprenticeship.ContactWebsite,
                    DeliveryMethod = "Employer Based",
                    VenueName = string.Empty,
                    Radius = string.Empty,
                    YourVenueReference = string.Empty,
                    DeliveryModes = "Employer Address",
                    NationalDelivery = "No",
                    SubRegion = "County Durham"
                },
                new CsvApprenticeshipRow()
                {
                    StandardCode = standard.StandardCode.ToString(),
                    StandardVersion = standard.Version.ToString(),
                    ApprenticeshipInformation = apprenticeship.MarketingInformation,
                    ApprenticeshipWebpage = apprenticeship.ApprenticeshipWebsite,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactPhone = apprenticeship.ContactTelephone,
                    ContactUrl = apprenticeship.ContactWebsite,
                    DeliveryMethod = "Classroom Based",
                    VenueName = venue.VenueName,
                    Radius = "25",
                    YourVenueReference = venue.ProviderVenueRef,
                    DeliveryModes = "Day Release;Block Release",
                    NationalDelivery = string.Empty,
                    SubRegion = string.Empty
                },
                new CsvApprenticeshipRow()
                {
                    StandardCode = standard.StandardCode.ToString(),
                    StandardVersion = standard.Version.ToString(),
                    ApprenticeshipInformation = apprenticeship.MarketingInformation,
                    ApprenticeshipWebpage = apprenticeship.ApprenticeshipWebsite,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactPhone = apprenticeship.ContactTelephone,
                    ContactUrl = apprenticeship.ContactWebsite,
                    DeliveryMethod = "Classroom Based",
                    VenueName = venue.VenueName,
                    Radius = "50",
                    YourVenueReference = venue.ProviderVenueRef,
                    DeliveryModes = "Employer Address;Day Release;Block Release",
                    NationalDelivery = string.Empty,
                    SubRegion = string.Empty
                }
            });
        }

        [Fact]
        public async Task Get_ClassroomLocationWithEmptyRadius_HasDefaultRadiusInDownload()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");

            var standard = await TestData.CreateStandard();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), providerVenueRef: "VENUE_REF");

            var apprenticeship = await TestData.CreateApprenticeship(
                providerId: provider.ProviderId,
                standard: standard,
                createdBy: User.ToUserInfo(),
                locations: new[]
                {
                    new CreateApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.DayRelease, ApprenticeshipDeliveryMode.BlockRelease },
                        VenueId = venue.VenueId,
                        Radius = null
                    }
                });

            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/apprenticeships/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_apprenticeships_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            var rows = csvReader.GetRecords<CsvApprenticeshipRow>();
            rows.Single().Radius.Should().Be("30");
        }

        [Fact]
        public async Task Get_BothLocationWithNationalAndRadius_HasEmptyRadiusInDownload()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");

            var standard = await TestData.CreateStandard();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), providerVenueRef: "VENUE_REF");

            var apprenticeship = await TestData.CreateApprenticeship(
                providerId: provider.ProviderId,
                standard: standard,
                createdBy: User.ToUserInfo(),
                locations: new[]
                {
                    new CreateApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                        DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress, ApprenticeshipDeliveryMode.BlockRelease },
                        VenueId = venue.VenueId,
                        National = true,
                        Radius = 600
                    }
                });

            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/apprenticeships/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_apprenticeships_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            var row = csvReader.GetRecords<CsvApprenticeshipRow>().Single();
            row.Radius.Should().Be("");
            row.NationalDelivery.Should().Be("Yes");
        }
    }
}

