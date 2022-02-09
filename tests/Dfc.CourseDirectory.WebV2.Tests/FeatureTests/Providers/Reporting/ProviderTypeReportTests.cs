using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
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
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task ProviderTypeReport_Get_WithAdminUser_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var providers = await Task.WhenAll(
                CreateProvider(1, ProviderType.None, ProviderStatus.Unregistered, "Provider deactivated, not verified"),
                CreateProvider(2, ProviderType.None, ProviderStatus.Registered, "Active"),
                CreateProvider(3, ProviderType.None, ProviderStatus.Onboarded, "Active"),
                CreateProvider(4, ProviderType.FE, ProviderStatus.Onboarded, "Active"),
                CreateProvider(5, ProviderType.Apprenticeships, ProviderStatus.Onboarded, "Active"),
                CreateProvider(6, ProviderType.TLevels, ProviderStatus.Onboarded, "Active"),
                CreateProvider(7, ProviderType.FE | ProviderType.Apprenticeships, ProviderStatus.Onboarded, "Active"),
                CreateProvider(8, ProviderType.FE | ProviderType.TLevels, ProviderStatus.Onboarded, "Active"),
                CreateProvider(9, ProviderType.Apprenticeships | ProviderType.TLevels, ProviderStatus.Onboarded, "Active"),
                CreateProvider(10, ProviderType.FE | ProviderType.Apprenticeships | ProviderType.TLevels, ProviderStatus.Onboarded, "Active"));

            await User.AsTestUser(userType);
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
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
            csvReader.GetFieldIndex("Other Course Count", isTryGet: true).Should().Be(8);
            csvReader.GetFieldIndex("Live Apprenticeship Count", isTryGet: true).Should().Be(9);
            csvReader.GetFieldIndex("Other Apprenticeship Count", isTryGet: true).Should().Be(10);
            csvReader.GetFieldIndex("Live T Level Count", isTryGet: true).Should().Be(11);
            csvReader.GetFieldIndex("Other T Level Count", isTryGet: true).Should().Be(12);

            var records = csvReader.GetRecords<Features.Providers.Reporting.ProviderTypeReport.Csv>().ToArray();
            records.Length.Should().Be(10);

            using (new AssertionScope())
            {
                foreach (var record in records)
                {

                    var provider = providers.SingleOrDefault(p => p.Ukprn == record.ProviderUkprn);
                    provider.Should().NotBeNull();
                    record.ProviderName.Should().Be(provider.Name);
                    record.ProviderType.Should().Be((int)provider.ProviderType);
                    record.ProviderTypeDescription.Should().Be(string.Join("; ",
                        Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>()
                            .Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p))
                            .DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                    record.ProviderStatus.Should().Be((int)provider.ProviderStatus);
                    record.ProviderStatusDescription.Should().Be(provider.ProviderStatus.ToString());
                    record.UkrlpProviderStatus.Should().Be(provider.UkrlpProviderStatusDescription);
                    record.LiveCourseCount.Should().Be(0);
                    record.OtherCourseCount.Should().Be(0);
                    record.LiveApprenticeshipCount.Should().Be(0);
                    record.OtherApprenticeshipCount.Should().Be(0);
                    record.LiveTLevelCount.Should().Be(0); 
                    record.OtherTLevelCount.Should().Be(0);
                    
                }
            }
        }

        [Fact]
        public async Task ProviderTypeReport_Get_WithCounts_ReturnsExpectedCsv()
        {
            var provider = await CreateProvider(12345678, ProviderType.FE | ProviderType.Apprenticeships | ProviderType.TLevels, ProviderStatus.Onboarded, "Active");
            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "TestVenue1");

            var liveCourseIds = await Task.WhenAll(
                Enumerable.Range(0, 3).Select(_ => TestData.CreateCourse(provider.ProviderId, User.ToUserInfo())));

            var archivedCourseIds = await Task.WhenAll(
                Enumerable.Range(0, 1).Select(async _ =>
                {
                    var course = await TestData.CreateCourse(provider.ProviderId, User.ToUserInfo(), configureCourseRuns: builder => builder.WithCourseRun());

                    await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteCourseRun()
                    {
                        CourseId = course.CourseId,
                        CourseRunId = course.CourseRuns.Single().CourseRunId,
                        DeletedBy = User.ToUserInfo(),
                        DeletedOn = Clock.UtcNow
                    }));

                    return course.CourseId;
                }));

            var standard = await TestData.CreateStandard(123, 456, "TestStandard1");
            var liveApprenticeships = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => TestData.CreateApprenticeship(provider.ProviderId, standard, User.ToUserInfo())));

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var tLevels = await Task.WhenAll(Enumerable.Range(0, 5).Select(i =>
                TestData.CreateTLevel(provider.ProviderId, tLevelDefinitions.OrderBy(_ => Guid.NewGuid()).Select(d => d.TLevelDefinitionId).First(), new[] { venue.VenueId }, User.ToUserInfo(), startDate: Clock.UtcNow.AddDays(i).Date)));

            var deletedTLevels = tLevels.OrderBy(_ => Guid.NewGuid()).Take(2).ToArray();
            await Task.WhenAll(deletedTLevels.Select(t => TestData.DeleteTLevel(t.TLevelId, User.ToUserInfo())));

            var liveTLevels = tLevels.Except(deletedTLevels).ToArray();

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/providers/reports/provider-type");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=ProviderTypeReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var record = csvReader.GetRecords<Features.Providers.Reporting.ProviderTypeReport.Csv>().SingleOrDefault();

            using (new AssertionScope())
            {
                record.Should().NotBeNull();
                record.ProviderName.Should().Be(provider.Name);
                record.ProviderType.Should().Be((int)provider.ProviderType);
                record.ProviderTypeDescription.Should().Be(string.Join("; ", Enum.GetValues(typeof(ProviderType)).Cast<ProviderType>().Where(p => p != ProviderType.None && provider.ProviderType.HasFlag(p)).DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())));
                record.ProviderStatus.Should().Be((int)provider.ProviderStatus);
                record.ProviderStatusDescription.Should().Be(provider.ProviderStatus.ToString());
                record.UkrlpProviderStatus.Should().Be(provider.UkrlpProviderStatusDescription);
                record.LiveCourseCount.Should().Be(liveCourseIds.Length);
                record.OtherCourseCount.Should().Be(archivedCourseIds.Length);
                record.LiveApprenticeshipCount.Should().Be(liveApprenticeships.Length);
                record.OtherApprenticeshipCount.Should().Be(0);
                record.LiveTLevelCount.Should().Be(liveTLevels.Length);
                record.OtherTLevelCount.Should().Be(deletedTLevels.Length);
            }
        }

        private async Task<(Guid ProviderId, int Ukprn, string Name, ProviderType ProviderType, ProviderStatus ProviderStatus, string UkrlpProviderStatusDescription)> CreateProvider(
            int index,
            ProviderType providerType,
            ProviderStatus providerStatus,
            string ukrlpProviderStatusDescription)
        {
            var providerName = $"TestProvider{index}";
            var provider = await TestData.CreateProvider(providerName, providerType, ukrlpProviderStatusDescription, status: providerStatus);

            return (provider.ProviderId, provider.Ukprn, providerName, providerType, providerStatus, ukrlpProviderStatusDescription);
        }
    }
}
