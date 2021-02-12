using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class TLevelDefinitionsTests : TestBase
    {
        public TLevelDefinitionsTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ValidRequest_ReturnsOkWithExpectedOutput()
        {
            // Arrange
            var tLevelDefinitions = new[]
            {
                CreateTLevelDefinition(1),
                CreateTLevelDefinition(2),
                CreateTLevelDefinition(3)
            };

            SqlQueryDispatcher
                .Setup(s => s.ExecuteQuery(It.IsAny<GetTLevelDefinitions>()))
                .ReturnsAsync(tLevelDefinitions);

            // Act
            var response = await HttpClient.GetAsync($"tleveldefinitions");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            json["tLevelDefinitions"]?.ToArray().Should().NotBeNullOrEmpty()
                .And.HaveCount(tLevelDefinitions.Length);

            using (new AssertionScope())
            {
                foreach (var tLevelDefinition in tLevelDefinitions)
                {
                    var item = json["tLevelDefinitions"].SingleOrDefault(t => t["tLevelDefinitionId"].ToObject<Guid>() == tLevelDefinition.TLevelDefinitionId);
                    item.Should().NotBeNull();
                    item["frameworkCode"].ToObject<int>().Should().Be(tLevelDefinition.FrameworkCode);
                    item["progType"].ToObject<int>().Should().Be(tLevelDefinition.ProgType);
                    item["qualificationLevel"].ToObject<string>().Should().Be(tLevelDefinition.QualificationLevel.ToString());
                    item["name"].ToObject<string>().Should().Be(tLevelDefinition.Name);
                }
            }
        }

        private static TLevelDefinition CreateTLevelDefinition(int seed = 1) =>
            new TLevelDefinition
            {
                TLevelDefinitionId = Guid.NewGuid(),
                FrameworkCode = 123 * seed,
                ProgType = 456 * seed,
                QualificationLevel = 789 * seed,
                Name = $"TestTLevelDefinition{seed}"
            };
    }
}
