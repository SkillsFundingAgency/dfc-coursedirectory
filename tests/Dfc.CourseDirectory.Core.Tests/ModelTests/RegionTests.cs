using System;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ModelTests
{
    public class RegionTests
    {
        [Fact]
        public void Reduce_ReturnsCorrectRegions()
        {
            // Arrange
            var subRegion1a = CreateRegion("1a");
            var region1 = CreateRegion("1", subRegion1a);

            var subRegion2a = CreateRegion("2a");
            var subRegion2b = CreateRegion("2b");
            var region2 = CreateRegion("2", subRegion2a, subRegion2b);

            var subRegion3a = CreateRegion("3a");
            var subRegion3b = CreateRegion("3b");
            var region3 = CreateRegion("3", subRegion3a, subRegion3b);

            var subRegion4a = CreateRegion("4a");
            var subRegion4b = CreateRegion("4b");
            var region4 = CreateRegion("4", subRegion4a, subRegion4b);

            var subRegion5a = CreateRegion("5a");
            var subRegion5b = CreateRegion("5b");
            var region5 = CreateRegion("5", subRegion5a, subRegion5b);

            var allRegions = new[]
            {
                region1,
                region2,
                region3,
                region4,
                region5
            };

            var regionIds = new[]
            {
                "1",  // A top-level region
                "1a", // A sub-region of an already-specified top-level region
                "2a", // A sub-region of another top-level region
                "2b", // A sub-region of another top-level region, completing the set of all sub-regions for the region
                "3a", // A sub-region of yet another top-level region, but it's the only sub-region for this region
                "4",
                "4",  // A duplicate top-level region
                "5a",
                "5a", // A duplicate sub-region
            };

            // Act
            var result = Region.Reduce(allRegions, regionIds);

            // Assert
            result.Should().BeEquivalentTo(new[]
            {
                region1,
                region2,
                subRegion3a,
                region4,
                subRegion5a
            });

            static Region CreateRegion(string id, params Region[] subRegions) => new Region()
            {
                Id = id,
                Name = $"Region {id}",
                SubRegions = subRegions ?? Array.Empty<Region>()
            };
        }
    }
}
