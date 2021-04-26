using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers.Reporting
{
    public class ProviderMissingPrimaryContactReportTests : MvcTestBase
    {
        public ProviderMissingPrimaryContactReportTests(CourseDirectoryApplicationFactory factory)
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
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderWithActiveCoursesAnd_NoContactP_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType");
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(1);
        }

        [Theory]
        [InlineData(null, "street", null, null)]
        [InlineData("CV17 9AD", null, null, null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveApprenticeships_ReturnsExpectedCsv(string postcode, string saonDescription, string paonDescription, string addressPaonDescription)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
            {
                CreateContact(postcode, saonDescription, paonDescription, addressPaonDescription)
            });
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider.ProviderId, standardOrFramework: standard, createdBy: User.ToUserInfo(), status: ApprenticeshipStatus.Live);

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(1);

            using (new AssertionScope())
            {
                foreach (var record in records)
                {
                    provider.Should().NotBeNull();
                    record.ProviderUkprn.Should().Be(provider.Ukprn);
                    record.ProviderName.Should().Be(provider.ProviderName);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Onboarded);
                }
            }
        }

        [Theory]
        [InlineData(null, "street", null, null)]
        [InlineData("CV17 9AD", null, null, null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveTLevels_ReturnsExpectedCsv(string postcode, string saonDescription, string paonDescription, string addressPaonDescription)
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact(postcode, saonDescription, paonDescription, addressPaonDescription)
                });

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;
            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var tLevel = await TestData.CreateTLevel(
               provider.ProviderId,
               tLevelDefinitionId,
               locationVenueIds: new[] { venueId },
               createdBy: User.ToUserInfo());

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(1);

            using (new AssertionScope())
            {
                foreach (var record in records)
                {
                    provider.Should().NotBeNull();
                    record.ProviderUkprn.Should().Be(provider.Ukprn);
                    record.ProviderName.Should().Be(provider.ProviderName);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Onboarded);
                }
            }
        }

        [Theory]
        [InlineData(null, "street", null, null)]
        [InlineData("CV17 9AD", null, null, null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveCourse_ReturnsExpectedCsv(string postcode, string saonDescription, string paonDescription, string addressPaonDescription)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
                {
                    CreateContact(postcode, saonDescription, paonDescription, addressPaonDescription)
                });
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(1);
            using (new AssertionScope())
            {
                foreach (var record in records)
                {
                    provider.Should().NotBeNull();
                    record.ProviderUkprn.Should().Be(provider.Ukprn);
                    record.ProviderName.Should().Be(provider.ProviderName);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Onboarded);
                }
            }
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_CsvHeaderIsCorrect()
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
                {
                    CreateContact("CV12 1AA", "Some street1","some street",null)
                });
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();

            csvReader.ReadHeader().Should().BeTrue();
            csvReader.GetFieldIndex("UKPRN", isTryGet: true).Should().Be(0);
            csvReader.GetFieldIndex("Provider Name", isTryGet: true).Should().Be(1);
            csvReader.GetFieldIndex("Provider Status", isTryGet: true).Should().Be(2);
            csvReader.GetFieldIndex("Provider Type", isTryGet: true).Should().Be(3);
            csvReader.GetFieldIndex("Provider Type Description", isTryGet: true).Should().Be(4);
        }

        [Theory]
        [InlineData("CV17 9AD", "street", null, null)]
        [InlineData("CV17 9AD", null, "street",null)]
        [InlineData("CV17 9AD", null, null, "street")]
        [InlineData("CV17 9AD", "street", null, "street")]
        [InlineData("CV17 9AD", "street", "street", "street")]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveCourses_ReturnsEmptyCsv(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
                {
                    CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
                });
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Theory]
        [InlineData("CV17 9AD", "street", null, null)]
        [InlineData("CV17 9AD", null, "street", null)]
        [InlineData("CV17 9AD", null, null, "street")]
        [InlineData("CV17 9AD", "street", null, "street")]
        [InlineData("CV17 9AD", "street", "street", "street")]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveTLevels_ReturnsEmptyCsv(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
                });

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;
            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var tLevel = await TestData.CreateTLevel(
               provider.ProviderId,
               tLevelDefinitionId,
               locationVenueIds: new[] { venueId },
               createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Theory]
        [InlineData("CV17 9AD", "street", null, null)]
        [InlineData("CV17 9AD", null, "street", null)]
        [InlineData("CV17 9AD", null, null, "street")]
        [InlineData("CV17 9AD", "street", null, "street")]
        [InlineData("CV17 9AD", "street", "street", "street")]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveApprenticeships_ReturnsEmptyCsv(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
            });
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider.ProviderId, standardOrFramework: standard, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Theory]
        [InlineData("CV17 9AD", null, null, null)]
        [InlineData(null, null, "street", null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MultipleProvidersWithApprenticeshipsCoursesAndTLevels_ReturnsExpectedCsv(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            //Arange
            var provider1 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
            {
                CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
            });

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider1.ProviderId, standardOrFramework: standard, createdBy: User.ToUserInfo());

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider2 = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
                });

            var venueId = (await TestData.CreateVenue(provider2.ProviderId)).Id;
            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var tLevel = await TestData.CreateTLevel(
               provider2.ProviderId,
               tLevelDefinitionId,
               locationVenueIds: new[] { venueId },
               createdBy: User.ToUserInfo());

            var provider3 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
            {
                    CreateContact(postcode, addressSaonDescription, addressPaonDescription, addressStreetDescription)
            });
            await TestData.CreateCourse(provider3.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(3);
        }

        [Theory]
        [InlineData(ApprenticeshipStatus.Pending)]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProvidersWithNonLiveApprenticeships_ReturnsEmptyCsv(ApprenticeshipStatus apprenticeshipStatus)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact("CV4 1AA", null, null,"some street")
            });
            var standard1 = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider.ProviderId, standardOrFramework: standard1, createdBy: User.ToUserInfo(), status: apprenticeshipStatus);

            await User.AsHelpdesk();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Theory]
        [InlineData(CourseStatus.Pending)]
        [InlineData(CourseStatus.None)]
        [InlineData(CourseStatus.Deleted)]
        [InlineData(CourseStatus.Archived)]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProvidersWithNonLiveCourses_ReturnsEmptyCsv(CourseStatus courseStatus)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact("CV4 1AA", "some Street", null, null)
            });
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(), courseStatus: courseStatus);

            await User.AsHelpdesk();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderDetailsAreCorrect()
        {
            //Arange
            var provider = await TestData.CreateProvider("Some Provider Name", Core.Models.ProviderType.FE, "ProviderType");
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            using (new AssertionScope())
            {
                foreach (var record in records)
                {
                    provider.Should().NotBeNull();
                    record.ProviderUkprn.Should().Be(provider.Ukprn);
                    record.ProviderName.Should().Be(provider.ProviderName);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Onboarded);
                }
            }
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_LiveProvidersWithDeletedTLevel_ReturnsEmptyCsv()
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact("CV1 1AA", null, "some Street",null)
                });

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;
            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var tLevel = await TestData.CreateTLevel(
               provider.ProviderId,
               tLevelDefinitionId,
               locationVenueIds: new[] { venueId },
               createdBy: User.ToUserInfo());

            await User.AsHelpdesk();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(0);
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_MultipleProvidersWithMultipleApprenticeshipsCoursesAndTLevels_ReturnsExpectedCsvOrdedByUkprnAscending()
        {
            //Arange
            var provider1 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
            {
                CreateContact("CV1 1AA", null, null,null)
            });

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider1.ProviderId, standardOrFramework: standard, createdBy: User.ToUserInfo());
            var standard1 = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider1.ProviderId, standardOrFramework: standard1, createdBy: User.ToUserInfo());

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider2 = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact("CV2 1AA", null, null,null)
                });
            var venueId2= (await TestData.CreateVenue(provider2.ProviderId)).Id;
            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var tLevel = await TestData.CreateTLevel(
               provider2.ProviderId,
               tLevelDefinitionId,
               locationVenueIds: new[] { venueId2 },
               createdBy: User.ToUserInfo());

            var provider3 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", contacts: new[]
            {
                    CreateContact(null, "SOME STREET3", null,null)
            });
            await TestData.CreateCourse(provider3.ProviderId, createdBy: User.ToUserInfo());
            await TestData.CreateCourse(provider3.ProviderId, createdBy: User.ToUserInfo());
            await User.AsHelpdesk();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/providers-missing-primary-contact");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderMissingPrimaryContactReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader().Should().BeTrue();
            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>().ToArray();
            records.Length.Should().Be(3);
            records.Should().BeInAscendingOrder(x => x.ProviderUkprn);
        }

        private CreateProviderContact CreateContact(string postcode, string addressSaonDescription, string addressPaonDescription, string addressStreetDescription)
        {
            return new CreateProviderContact()
            {
                ContactType = "P",
                AddressSaonDescription = addressSaonDescription,
                AddressPaonDescription = addressPaonDescription,
                AddressStreetDescription = addressStreetDescription,
                AddressLocality = "The Town",
                AddressItems = new List<string>()
                        {
                            "United Kingdom"
                        },
                AddressPostCode = postcode,
                ContactEmail = "email@provider1.com",
                ContactTelephone1 = "01234 567890",
                ContactWebsiteAddress = "provider1.com",
                PersonalDetailsGivenName = "The",
                PersonalDetailsFamilyName = "Contact"
            };
        }
    }
}

