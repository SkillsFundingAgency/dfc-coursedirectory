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
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered);
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

            using (new AssertionScope())
            {
                foreach (var record in records)
                {
                    provider.Should().NotBeNull();
                    record.ProviderUkprn.Should().Be(provider.Ukprn);
                    record.ProviderName.Should().Be(provider.ProviderName);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Registered);
                }
            }
        }

        [Theory]
        [InlineData(null, "street")]
        [InlineData("CV17 9AD", null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveApprenticeships_ReturnsExpectedCsv(string postcode, string addressLine1)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact(postcode, addressLine1)
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
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Registered);
                }
            }
        }

        [Theory]
        [InlineData(null, "street")]
        [InlineData("CV17 9AD", null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveTLevels_ReturnsExpectedCsv(string postcode, string addressLine1)
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact(postcode, addressLine1)
                });

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);
            var tLevel1 = await TestData.CreateTLevel(
               provider.ProviderId,
               keepingTLevelDefinitionId,
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
        [InlineData(null, "street")]
        [InlineData("CV17 9AD", null)]
        public async Task ProvidersMissingPrimaryContactReport_Get_MissingContactDetailsLiveCourse_ReturnsExpectedCsv(string postcode, string addressLine1)
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
                {
                    CreateContact(postcode, addressLine1)
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
                    record.ProviderStatus.Should().Be((int)ProviderStatus.Registered);
                }
            }
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_CsvHeaderIsCorrect()
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
                {
                    CreateContact("CV12 1AA", "Some street1")
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
            csvReader.GetFieldIndex("Provider Type", isTryGet: true).Should().Be(2);
            csvReader.GetFieldIndex("Provider Type Description", isTryGet: true).Should().Be(3);
            csvReader.GetFieldIndex("Provider Status", isTryGet: true).Should().Be(4);
        }

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveCourses_ReturnsEmptyCsv()
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
                {
                    CreateContact("CV1 1AA", "SOME STREET")
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

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveTLevels_ReturnsEmptyCsv()
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact("CV1 1AA", "Some road")
                });

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);
            var tLevel1 = await TestData.CreateTLevel(
               provider.ProviderId,
               keepingTLevelDefinitionId,
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
        public async Task ProvidersMissingPrimaryContactReport_Get_ProviderHasValidContactTypePWithLiveApprenticeships_ReturnsEmptyCsv()
        {
            //Arange
            var provider = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact("CV1 1AA", "Some Street")
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

        [Fact]
        public async Task ProvidersMissingPrimaryContactReport_Get_MultipleProvidersWithApprenticeshipsCoursesAndTLevels_ReturnsExpectedCsv()
        {
            //Arange
            var provider1 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                CreateContact("CV1 1AA", null)
            });

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            await TestData.CreateApprenticeship(provider1.ProviderId, standardOrFramework: standard, createdBy: User.ToUserInfo());
            
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var provider2 = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray(), contacts: new[]
                {
                    CreateContact("CV2 1AA", null)
                });

            var venueId = (await TestData.CreateVenue(provider2.ProviderId)).Id;

            var keepingTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var removingTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            keepingTLevelDefinitionId.Should().NotBe(removingTLevelDefinitionId);
            var tLevel1 = await TestData.CreateTLevel(
               provider2.ProviderId,
               keepingTLevelDefinitionId,
               locationVenueIds: new[] { venueId },
               createdBy: User.ToUserInfo());

            var provider3 = await TestData.CreateProvider("providerName", Core.Models.ProviderType.None, "ProviderType", status: Core.Models.ProviderStatus.Registered, contacts: new[]
            {
                    CreateContact(null, "SOME STREET3")
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

        private CreateProviderContact CreateContact(string postcode, string addressLine1)
        {
            return new CreateProviderContact()
            {
                ContactType = "P",
                AddressSaonDescription = addressLine1,
                AddressPaonDescription = "2nd Line of Address",
                AddressStreetDescription = "The Street",
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

