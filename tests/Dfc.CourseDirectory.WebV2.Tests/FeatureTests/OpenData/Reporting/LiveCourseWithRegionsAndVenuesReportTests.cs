using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Reporting;
using Dfc.CourseDirectory.WebV2.Tests.Core;
using Dfc.CourseDirectory.WebV2.Tests.Data;
using Dfc.CourseDirectory.WebV2.ViewComponents.OpenData.Reporting;
using Dfc.CourseDirectory.WebV2.ViewComponents.OpenData.Reporting.LiveCoursesWithRegionsAndVenuesReport;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.OpenData.Reporting
{
    public class LiveCoursesWithRegionsAndVenuesReportTests : MvcTestBase
    {
        private const string LiveCoursesWithRegionsAndVenuesReportUrl = "/opendata/reports/live-courses-with-regions-and-venues-report";        
        private const string LiveCoursesWithRegionsAndVenuesReportNamePrefix = "LiveCoursesWithRegionsAndVenuesReport-";

        public LiveCoursesWithRegionsAndVenuesReportTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }
                
        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_WithProviderUser_ReturnsForbidden(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_WithAdminUser_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCoursesWithRegionsAndVenuesReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_CsvHeaderIsCorrect(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCoursesWithRegionsAndVenuesReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();

            csvReader.ReadHeader().Should().BeTrue();
            csvReader.GetFieldIndex("PROVIDER_UKPRN", isTryGet: true).Should().Be(0);
            csvReader.GetFieldIndex("COURSE_ID", isTryGet: true).Should().Be(1);
            csvReader.GetFieldIndex("COURSE_RUN_ID", isTryGet: true).Should().Be(2);
            csvReader.GetFieldIndex("LEARN_AIM_REF", isTryGet: true).Should().Be(3);
            csvReader.GetFieldIndex("COURSE_NAME", isTryGet: true).Should().Be(4);
            csvReader.GetFieldIndex("WHO_THIS_COURSE_IS_FOR", isTryGet: true).Should().Be(5);
            csvReader.GetFieldIndex("DELIVER_MODE", isTryGet: true).Should().Be(6);
            csvReader.GetFieldIndex("STUDY_MODE", isTryGet: true).Should().Be(7);
            csvReader.GetFieldIndex("ATTENDANCE_PATTERN", isTryGet: true).Should().Be(8);
            csvReader.GetFieldIndex("FLEXIBLE_STARTDATE", isTryGet: true).Should().Be(9);
            csvReader.GetFieldIndex("STARTDATE", isTryGet: true).Should().Be(10);
            csvReader.GetFieldIndex("DURATION_UNIT", isTryGet: true).Should().Be(11);
            csvReader.GetFieldIndex("DURATION_VALUE", isTryGet: true).Should().Be(12);
            csvReader.GetFieldIndex("COST", isTryGet: true).Should().Be(13);            
            csvReader.GetFieldIndex("COST_DESCRIPTION", isTryGet: true).Should().Be(14);
            csvReader.GetFieldIndex("NATIONAL", isTryGet: true).Should().Be(15);
            csvReader.GetFieldIndex("REGIONS", isTryGet: true).Should().Be(16);
            csvReader.GetFieldIndex("LOCATION_NAME", isTryGet: true).Should().Be(17);
            csvReader.GetFieldIndex("LOCATION_ADDRESS1", isTryGet: true).Should().Be(18);
            csvReader.GetFieldIndex("LOCATION_ADDRESS2", isTryGet: true).Should().Be(19);
            csvReader.GetFieldIndex("LOCATION_COUNTY", isTryGet: true).Should().Be(20);
            csvReader.GetFieldIndex("LOCATION_EMAIL", isTryGet: true).Should().Be(21);
            csvReader.GetFieldIndex("LOCATION_LATITUDE", isTryGet: true).Should().Be(22);
            csvReader.GetFieldIndex("LOCATION_LONGITUDE", isTryGet: true).Should().Be(23);            
            csvReader.GetFieldIndex("LOCATION_POSTCODE", isTryGet: true).Should().Be(24);
            csvReader.GetFieldIndex("LOCATION_TELEPHONE", isTryGet: true).Should().Be(25);
            csvReader.GetFieldIndex("LOCATION_TOWN", isTryGet: true).Should().Be(26);
            csvReader.GetFieldIndex("LOCATION_WEBSITE", isTryGet: true).Should().Be(27);
            csvReader.GetFieldIndex("COURSE_URL", isTryGet: true).Should().Be(28);
            csvReader.GetFieldIndex("UPDATED_DATE", isTryGet: true).Should().Be(29);
            csvReader.GetFieldIndex("ENTRY_REQUIREMENTS", isTryGet: true).Should().Be(30);
            csvReader.GetFieldIndex("HOW_YOU_WILL_BE_ASSESSED", isTryGet: true).Should().Be(31);
            csvReader.GetFieldIndex("COURSE_TYPE", isTryGet: true).Should().Be(32);
            csvReader.GetFieldIndex("SECTOR", isTryGet: true).Should().Be(33);
            csvReader.GetFieldIndex("EDUCATION_LEVEL", isTryGet: true).Should().Be(34);
            csvReader.GetFieldIndex("AWARDING_BODY", isTryGet: true).Should().Be(35);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_ProviderHasOutOfDateCourse_ReturnsEmptyCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));

            var course1 = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: builder =>
                {
                    builder.WithCourseRun(startDate: Clock.UtcNow.Date.AddDays(-60));
                });

            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCoursesWithRegionsAndVenuesReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_ProviderHasNonLarsCourses_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.NonLARS, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.AddSectors();
            await TestData.CreateNonLarsCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCoursesWithRegionsAndVenuesReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCoursesWithRegionsAndVenuesReport_Get_ProviderHasNoLiveCourses_ReturnsEmptyCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCoursesWithRegionsAndVenuesReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCoursesWithRegionsAndVenuesReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(0);
        }        

        private ProviderContact CreateContact(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            return new ProviderContact()
            {
                ContactType = "P",
                AddressSaonDescription = addressSaonDescription,
                AddressPaonDescription = addressPaonDescription,
                AddressStreetDescription = addressStreetDescription,
                AddressLocality = "The Town",
                AddressItems = "United Kingdom",
                AddressPostcode = postcode,
                Email = "email@provider1.com",
                Telephone1 = "01234 567890",
                WebsiteAddress = "provider1.com",
                PersonalDetailsPersonNameGivenName = "The",
                PersonalDetailsPersonNameFamilyName = "Contact"
            };
        }
    }
}
