using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Reporting.ProviderTypeReport;
using Dfc.CourseDirectory.WebV2.Tests.Core;
using Dfc.CourseDirectory.WebV2.Tests.Data;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers.Reporting
{
    public class ProviderTypeReportTests : MvcTestBase
    {
        public ProviderTypeReportTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderTypeReport_Get_WithProviderUser_ReturnsForbidden(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProviderTypeReport_Get_WithAdminUser_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

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
        public async Task ProviderTypeReport_Get_CsvHeaderIsCorrect(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();

            csvReader.ReadHeader().Should().BeTrue();
            csvReader.GetFieldIndex("UKPRN", isTryGet: true).Should().Be(0);
            csvReader.GetFieldIndex("Provider Name", isTryGet: true).Should().Be(1);
            csvReader.GetFieldIndex("Provider Type", isTryGet: true).Should().Be(2);
            csvReader.GetFieldIndex("Provider Type Description", isTryGet: true).Should().Be(3);
            csvReader.GetFieldIndex("Provider Status", isTryGet: true).Should().Be(4);
            csvReader.GetFieldIndex("CD Provider Status", isTryGet: true).Should().Be(5);
            csvReader.GetFieldIndex("Ukrlp Provider Status", isTryGet: true).Should().Be(6);
            csvReader.GetFieldIndex("Live Course Count", isTryGet: true).Should().Be(7);
            csvReader.GetFieldIndex("Live T Level Count", isTryGet: true).Should().Be(8);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProviderTypeReport_Get_ProviderHasNoLiveCourses_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);
            records[0].LiveCourseCount.Should().Be(0);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProviderTypeReport_Get_ProviderIsOfTypeNone_ReturnsEmptyCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.None, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

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
        public async Task ProviderTypeReport_Get_ProviderHasNonLarsCourses_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.NonLARS, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.AddSectors();
            await TestData.CreateNonLarsCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);
            records[0].LiveCourseCount.Should().Be(1);

        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProviderTypeReport_Get_ProviderTypeTLevelOnly_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arrange
            var provider = await TestData.CreateProvider("providerName", ProviderType.TLevels, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            //Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);
            records[0].LiveCourseCount.Should().Be(1);

        }

        [Theory]
        [InlineData(TestUserType.Developer, ProviderType.FE)]
        [InlineData(TestUserType.Helpdesk, ProviderType.FE)]
        [InlineData(TestUserType.Developer, ProviderType.FE | ProviderType.TLevels)]
        [InlineData(TestUserType.Helpdesk, ProviderType.FE | ProviderType.TLevels)]
        public async Task ProviderTypeReport_Get_ProviderTypeContainingFECourses_ReturnsExpectedCsv(TestUserType userType, ProviderType providerType)
        {
            //Arrange
            var provider = await TestData.CreateProvider("providerName", providerType, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            //Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

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
        public async Task ProviderTypeReport_Get_ProviderHasOutOfDateCourse_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));

            var course1 = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: builder =>
                {
                    builder.WithCourseRun(startDate: Clock.UtcNow.Date.AddDays(-60));  // Expired
                });

            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Csv>().ToArray();
            records.Length.Should().Be(1);

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
