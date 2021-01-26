using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Functions.FixVenues;
using Dfc.CourseDirectory.Functions.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    public class VenueCorrectorTests
    {
        [Fact]
        public void VenueCorrector_WithNoCorrections_MakesNoChanges()
        {
            // Arrange
            var originalVenueId = new Guid("8473335C-58AB-4A58-8B49-6565617BB9B7");
            var locationId = new Guid("4D6746FF-7A79-48A9-A028-DAE8D40E4DA0");
            var clock = new FixedClock();
            var venueCorrector = new VenueCorrector(clock);
            var originalUpdatedDate = new DateTime(1991, 12, 21, 23, 59, 58);
            var originalUpdatedBy = "no-one";
            var apprenticeshipVenueCorrection = new ApprenticeshipVenueCorrection
            {
                Apprenticeship = new Apprenticeship
                {
                    ApprenticeshipLocations = new List<ApprenticeshipLocation>
                    {
                        new ApprenticeshipLocation
                        {
                            Id = locationId,
                            VenueId = originalVenueId,
                        }
                    },
                    UpdatedBy = originalUpdatedBy,
                    UpdatedDate = originalUpdatedDate,
                },
                ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                {
                    new ApprenticeshipLocationVenueCorrection
                    {
                        LocationId = locationId,
                        VenueCorrection = null,
                    }
                }
            };

            // Act
            var applyReturnValue = venueCorrector.Apply(apprenticeshipVenueCorrection);

            // Assert
            apprenticeshipVenueCorrection.Apprenticeship.ApprenticeshipLocations.Single().VenueId.Should()
                .Be(originalVenueId);
            apprenticeshipVenueCorrection.Apprenticeship.UpdatedBy.Should().Be(originalUpdatedBy);
            apprenticeshipVenueCorrection.Apprenticeship.UpdatedDate.Should().Be(originalUpdatedDate);
            applyReturnValue.Should().BeFalse("no changes requested");
        }

        [Fact]
        public void VenueCorrector_WithCorrections_SetsVenueIdAndAudit()
        {
            // Arrange
            var clock = new FixedClock();
            var venueCorrector = new VenueCorrector(clock);
            var venueCorrection = new VenueBuilder().Build();
            var locationToFix = new ApprenticeshipLocation
            {
                Id = new Guid("30146B96-F1FC-4B16-A046-D6A3B59CF1CE"),
                VenueId = Guid.Empty,
            };
            var decoy1 = new ApprenticeshipLocation // decoy
            {
                Id = new Guid("E08B4EE9-9AA3-41EC-BAF5-7961416E9A82"),
                VenueId = Guid.Empty,
            };
            var decoy2 = new ApprenticeshipLocation // decoy
            {
                Id = new Guid("E129D3CC-D141-49B9-94FC-E472A2F93A56"),
                VenueId = Guid.Empty,
            };
            var apprenticeshipVenueCorrection = new ApprenticeshipVenueCorrection
            {
                Apprenticeship = new Apprenticeship
                {
                    ApprenticeshipLocations = new List<ApprenticeshipLocation>
                    {
                        decoy1,
                        locationToFix,
                        decoy2,
                    },
                },
                ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                {
                    new ApprenticeshipLocationVenueCorrection
                    {
                        LocationId = locationToFix.Id,
                        VenueCorrection = venueCorrection,
                    }
                }
            };

            // Act
            var applyReturnValue = venueCorrector.Apply(apprenticeshipVenueCorrection);

            // Assert
            apprenticeshipVenueCorrection.Apprenticeship.ApprenticeshipLocations.Should().BeEquivalentTo(decoy1, locationToFix, decoy2);
            locationToFix.VenueId.Should().Be(venueCorrection.Id);
            locationToFix.UpdatedBy.Should().Be("VenueCorrector");
            locationToFix.UpdatedDate.Should().Be(clock.UtcNow);
            decoy1.VenueId.Should().Be(Guid.Empty);
            decoy1.UpdatedBy.Should().BeNull();
            decoy1.UpdatedDate.Should().BeNull();
            decoy2.VenueId.Should().Be(Guid.Empty);
            decoy2.UpdatedBy.Should().BeNull();
            decoy2.UpdatedDate.Should().BeNull();
            apprenticeshipVenueCorrection.Apprenticeship.UpdatedBy.Should().Be("VenueCorrector");
            apprenticeshipVenueCorrection.Apprenticeship.UpdatedDate.Should().Be(clock.UtcNow);
            applyReturnValue.Should().BeTrue("changes were applied");
        }
    }
}
