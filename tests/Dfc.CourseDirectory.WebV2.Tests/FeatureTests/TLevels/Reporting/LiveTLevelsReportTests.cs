using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.Reporting
{
    public class LiveTLevelsReportTests : MvcTestBase
    {
        public LiveTLevelsReportTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task LiveTLevelsReport_WithProviderUser_ReturnsForbidden(TestUserType userType)
        {
            //Arange
            var providerId = await TestData.CreateProvider();
            await User.AsTestUser(userType, providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/reports/live-t-levels");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task LiveTLevelsReport_WithAdminUser_ReturnsExpectedCsv(TestUserType userType)
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providers = await Task.WhenAll(Enumerable.Range(0, 3).Select(async i =>
            {
                var providerName = $"TestProvider{i}";

                var providerId = await TestData.CreateProvider(ukprn: i, providerName: providerName, providerType: ProviderType.TLevels, tLevelDefinitionIds: tLevelDefinitions.Select(d => d.TLevelDefinitionId).ToArray());

                return new { ProviderId = providerId, Ukprn = i, ProviderName = providerName };
            }));

            var tLevels = (await Task.WhenAll(providers.Select(async (p, i) =>
            {
                var user = await TestData.CreateUser($"TestUser{p.ProviderId}-{i}");
                var venues = await Task.WhenAll(Enumerable.Range(0, 3).Select(ii => TestData.CreateVenue(p.ProviderId, venueName: $"TestVenue{p.ProviderId}-{ii}")));

                return await Task.WhenAll(venues.Select((v, ii) =>
                    TestData.CreateTLevel(p.ProviderId, tLevelDefinitions.OrderBy(_ => Guid.NewGuid()).Select(d => d.TLevelDefinitionId).First(), new[] { v.Id }, user, startDate: Clock.UtcNow.AddDays(ii).Date)));
            }))).SelectMany(t => t).ToArray();

            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/reports/live-t-levels");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.ToString().Should().Be($"attachment; filename=LiveTLevelsReport-{Clock.UtcNow:yyyyMMddHHmmss}.csv");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csvReader.GetRecords<Features.TLevels.Reporting.LiveTLevelsReport.Csv>();

            records.Should().BeEquivalentTo(tLevels.Select(t =>
            {
                var p = providers.Single(p => p.ProviderId == t.ProviderId);

                return new Features.TLevels.Reporting.LiveTLevelsReport.Csv
                {
                    ProviderUkprn = p.Ukprn,
                    ProviderName = p.ProviderName,
                    TLevelName = t.TLevelDefinition.Name,
                    VenueName = t.Locations.Single().VenueName,
                    StartDate = t.StartDate
                };
            }).OrderBy(r => r.ProviderUkprn).ThenBy(r => r.TLevelName).ThenBy(r => r.VenueName).ThenBy(r => r.StartDate));
        }
    }
}
