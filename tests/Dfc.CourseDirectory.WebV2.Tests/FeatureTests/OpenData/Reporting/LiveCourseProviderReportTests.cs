﻿using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.OpenData.Reporting
{
    public class LiveCourseProviderReportTests : MvcTestBase
    {
        private const string LiveCourseProviderReportUrl = "/opendata/reports/live-course-providers-report";
        private const string LiveCourseProviderReportNamePrefix = "LiveCourseProvidersReport-";
        public LiveCourseProviderReportTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        
        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task LiveCourseProviderReport_Get_WithProviderUser_ReturnsForbidden(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_WithAdminUser_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_CsvHeaderIsCorrect(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();

            csvReader.ReadHeader().Should().BeTrue();
            csvReader.GetFieldIndex("PROVIDER_UKPRN", isTryGet: true).Should().Be(0);
            csvReader.GetFieldIndex("PROVIDER_NAME", isTryGet: true).Should().Be(1);
            csvReader.GetFieldIndex("TRADING_NAME", isTryGet: true).Should().Be(2);
            csvReader.GetFieldIndex("CONTACT_ADDRESS1", isTryGet: true).Should().Be(3);
            csvReader.GetFieldIndex("CONTACT_ADDRESS2", isTryGet: true).Should().Be(4);
            csvReader.GetFieldIndex("CONTACT_TOWN", isTryGet: true).Should().Be(5);
            csvReader.GetFieldIndex("CONTACT_POSTCODE", isTryGet: true).Should().Be(6);
            csvReader.GetFieldIndex("CONTACT_WEBSITE", isTryGet: true).Should().Be(7);
            csvReader.GetFieldIndex("CONTACT_PHONE", isTryGet: true).Should().Be(8);
            csvReader.GetFieldIndex("CONTACT_EMAIL", isTryGet: true).Should().Be(9);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_ProviderHasNoLiveCourses_ReturnsEmptyCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", ProviderType.FE, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }


        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_ProviderHasNonLarsCourses_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.NonLARS, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.AddSectors();
            await TestData.CreateNonLarsCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/opendata/reports/live-course-providers-report");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=LiveCourseProvidersReport-{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_ProviderTypeTLevelOnly_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arrange
            var provider = await TestData.CreateProvider("providerName", ProviderType.TLevels, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            //Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer, ProviderType.FE)]
        [InlineData(TestUserType.Helpdesk, ProviderType.FE)]
        [InlineData(TestUserType.Developer, ProviderType.FE | ProviderType.TLevels)]
        [InlineData(TestUserType.Helpdesk, ProviderType.FE | ProviderType.TLevels)]
        public async Task LiveCourseProviderReport_Get_ProviderTypeContainingFECourses_ReturnsExpectedCsv(TestUserType userType, ProviderType providerType)
        {
            //Arrange
            var provider = await TestData.CreateProvider("providerName", providerType, "ProviderType", contact: CreateContact("CV17 9AD", null, null, null));
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                LiveCourseProviderReportUrl);

            //Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveCourseProviderReport_Get_ProviderHasOutOfDateCourse_ReturnsEmptyCsv(TestUserType userType)
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
                LiveCourseProviderReportUrl);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename={LiveCourseProviderReportNamePrefix}{Clock.UtcNow:yyyyMMdd}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.OpenData.Reporting.LiveCourseProvidersReport.Csv>().ToArray();
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
