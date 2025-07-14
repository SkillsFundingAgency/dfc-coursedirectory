using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Xunit;


namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class SectorsTests : TestBase
    {
        public SectorsTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Sectors_Get_WithNoSectors_ReturnsOkWithEmptyCollection()
        {
            //Arrange
            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetSectors>()))
                .ReturnsAsync(Array.Empty<Sector>());

            // Act
            var response = await HttpClient.GetAsync($"sectors");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseContent = await (response?.Content?.ReadAsStringAsync());
            var sectorsResult = JsonConvert.DeserializeObject<List<Sector>>(responseContent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            sectorsResult.Should().NotBeNull();
            sectorsResult.Count().Should().Be(0);
        }

        [Fact]
        public async Task TLevels_Get_WithSectors_ReturnsOkWithExpectedSectorsCollection()
        {
            //Arrange
            var sectors = new List<Sector> {
                new Sector { Id = 1, Code = "ENVIRONMENTAL", Description = "Agriculture, environmental and animal care" },
                new Sector { Id = 2, Code = "BUSINESSADMIN", Description = "Business and administration" },
                new Sector { Id = 3, Code = "CARE", Description = "Care services" }
            };

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetSectorsAttachedWithCourses>()))
                .ReturnsAsync(sectors);

            // Act
            var response = await HttpClient.GetAsync($"sectors");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseContent = await (response?.Content?.ReadAsStringAsync());
            var sectorsResult = JsonConvert.DeserializeObject<List<Sector>>(responseContent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            sectorsResult.Should().NotBeNull();
            sectorsResult.Count().Should().Be(sectors.Count);
        }
    }
}
