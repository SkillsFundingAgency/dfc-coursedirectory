using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Functions.FixVenues;
using Dfc.CourseDirectory.Functions.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    public class VenueAnalyserTests
    {
        private readonly VenueAnalyser _venueAnalyser;

        public VenueAnalyserTests()
        {
            _venueAnalyser = new VenueAnalyser(new VenueCorrectionFinder());
        }

        [Fact]
        public void TestVenueAnalyser_LocationWithMatchingVenue_ReturnsCorrectAnalysis()
        {
            // Arrange
            var venue = new VenueBuilder().Build();

            var apprenticeshipLocation = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                VenueId = new Guid("62915572-18E1-4780-849A-6050E78B5008"),
                Address = AddressFromVenue(venue)
            };

            var apprenticeship = new Apprenticeship
            {
                Id = new Guid("80D875B3-3A2C-41C0-96D5-39DADB84CF0D"),
                ApprenticeshipLocations = new List<ApprenticeshipLocation> { apprenticeshipLocation },
            };

            // Act
            var apprenticeshipVenueCorrections = _venueAnalyser.AnalyseApprenticeship(apprenticeship, new List<Venue> {venue});

            // Assert
            apprenticeshipVenueCorrections.Apprenticeship.Should().Be(apprenticeship);
            apprenticeshipVenueCorrections.UnfixableVenueReason.Should().BeNull();
            var apprenticeshipLocationVenueCorrection = apprenticeshipVenueCorrections.ApprenticeshipLocationVenueCorrections.Should().ContainSingle().Subject;
            apprenticeshipLocationVenueCorrection.LocationId.Should().Be(apprenticeshipLocation.Id);
            apprenticeshipLocationVenueCorrection.VenueIdOriginal.Should().Be(apprenticeshipLocation.VenueId);
            apprenticeshipLocationVenueCorrection.CorruptionType.Should().Be(CorruptionType.VenueNotInProvidersLiveVenueList);
            apprenticeshipLocationVenueCorrection.UnfixableLocationVenueReason.Should().BeNull();
            apprenticeshipLocationVenueCorrection.VenueCorrection.Should().Be(venue);
        }

        [Fact]
        public void TestVenueAnalyser_ValidLocation_ReturnsCorrectAnalysis()
        {
            // Arrange
            var venue = new VenueBuilder().Build();

            var apprenticeshipLocation = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                VenueId = venue.Id,
                Address = AddressFromVenue(venue),
            };

            var apprenticeship = new Apprenticeship
            {
                Id = new Guid("80D875B3-3A2C-41C0-96D5-39DADB84CF0D"),
                ApprenticeshipLocations = new List<ApprenticeshipLocation> { apprenticeshipLocation },
            };

            // Act
            var apprenticeshipVenueCorrections = _venueAnalyser.AnalyseApprenticeship(apprenticeship, new List<Venue> {venue});

            // Assert
            apprenticeshipVenueCorrections.Apprenticeship.Should().Be(apprenticeship);
            apprenticeshipVenueCorrections.UnfixableVenueReason.Should().BeNull();
            apprenticeshipVenueCorrections.ApprenticeshipLocationVenueCorrections.Should().BeEmpty("the record isn't corrupt");
        }

        [Fact]
        public void TestVenueAnalyser_LocationWithNoMatchingVenues_ReturnsCorrectAnalysis()
        {
            // Arrange
            var venues = new List<Venue> { new VenueBuilder().Build() };

            var apprenticeshipLocation = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                VenueId = new Guid("62915572-18E1-4780-849A-6050E78B5008"),
            };

            var apprenticeship = new Apprenticeship
            {
                Id = new Guid("80D875B3-3A2C-41C0-96D5-39DADB84CF0D"),
                ApprenticeshipLocations = new List<ApprenticeshipLocation> { apprenticeshipLocation },
            };

            // Act
            var apprenticeshipVenueCorrections = _venueAnalyser.AnalyseApprenticeship(apprenticeship, venues);

            // Assert
            apprenticeshipVenueCorrections.Apprenticeship.Should().Be(apprenticeship);
            apprenticeshipVenueCorrections.UnfixableVenueReason.Should().BeNull();
            var apprenticeshipLocationVenueCorrection = apprenticeshipVenueCorrections.ApprenticeshipLocationVenueCorrections.Should().ContainSingle().Subject;
            apprenticeshipLocationVenueCorrection.LocationId.Should().Be(apprenticeshipLocation.Id);
            apprenticeshipLocationVenueCorrection.VenueIdOriginal.Should().Be(apprenticeshipLocation.VenueId);
            apprenticeshipLocationVenueCorrection.CorruptionType.Should().Be(CorruptionType.VenueNotInProvidersLiveVenueList);
            apprenticeshipLocationVenueCorrection.VenueCorrection.Should().BeNull();
            apprenticeshipLocationVenueCorrection.UnfixableLocationVenueReason.Should().Be(UnfixableLocationVenueReasons.NoMatchingVenue);
        }

        [Fact]
        public void TestVenueAnalyser_WithNoVenues_ReturnsCorrectAnalysis()
        {
            // Arrange
            var emptyVenueList = new List<Venue>();
            var apprenticeship = new Apprenticeship
            {
                Id = new Guid("E4144731-E4B5-4951-974D-AD457AC464EC"),
            };

            // Act
            var apprenticeshipVenueCorrections = _venueAnalyser.AnalyseApprenticeship(apprenticeship, emptyVenueList);

            // Assert
            apprenticeshipVenueCorrections.Apprenticeship.Should().Be(apprenticeship);
            apprenticeshipVenueCorrections.ApprenticeshipLocationVenueCorrections.Should().BeEmpty();
            apprenticeshipVenueCorrections.UnfixableVenueReason.Should().Be(UnfixableApprenticeshipVenueReasons.ProviderHasNoLiveVenues);
        }

        [Fact]
        public void TestVenueAnalyser_WithDuplicateVenues_ReturnsCorrectAnalysis()
        {
            // Arrange
            var potentialMatch = new VenueBuilder()
                .WithLatitude(60) // fixed value to avoid randomly matching
                .WithLongitude(-1) // fixed value to avoid randomly matching
                .Build();

            var duplicateMatch = VenueCloner.CloneVenue(potentialMatch);

            var availableVenues = new List<Venue>
            {
                new VenueBuilder().Build(), // decoy venue to make sure it doesn't pick the wrong one
                potentialMatch,
                duplicateMatch,
            };

            var apprenticeshipLocation = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                VenueId = new Guid("F1724123-CCBA-4811-A816-128542299F87"),
                Address = AddressFromVenue(potentialMatch),
            };
            var apprenticeship = new Apprenticeship
            {
                Id = new Guid("4E75B9D5-CC97-42EE-B891-5420F84EBAE9"),
                ApprenticeshipLocations = new List<ApprenticeshipLocation> { apprenticeshipLocation },
            };

            // Act
            var apprenticeshipVenueCorrections = _venueAnalyser.AnalyseApprenticeship(apprenticeship, availableVenues);

            // Assert
            apprenticeshipVenueCorrections.Apprenticeship.Should().Be(apprenticeship);
            apprenticeshipVenueCorrections.UnfixableVenueReason.Should().Be(null);
            var apprenticeshipLocationVenueCorrection = apprenticeshipVenueCorrections.ApprenticeshipLocationVenueCorrections.Should().ContainSingle().Subject;
            apprenticeshipLocationVenueCorrection.LocationId.Should().Be(apprenticeshipLocation.Id);
            apprenticeshipLocationVenueCorrection.VenueIdOriginal.Should().Be(apprenticeshipLocation.VenueId);
            apprenticeshipLocationVenueCorrection.CorruptionType.Should().Be(CorruptionType.VenueNotInProvidersLiveVenueList);
            apprenticeshipLocationVenueCorrection.UnfixableLocationVenueReason.Should().Be(UnfixableLocationVenueReasons.DuplicateMatchingVenues);
            apprenticeshipLocationVenueCorrection.DuplicateVenues.Should().BeEquivalentTo(potentialMatch, duplicateMatch);
        }

        [Fact]
        public void TestVenueAnalyser_LocationFiltering()
        {
            var employer = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            };

            var classroom = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
            };

            var both = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
            };

            var archived = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Archived,
                LocationType = LocationType.Venue,
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            };

            var region = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.Region,
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            };

            var subRegion = new ApprenticeshipLocation
            {
                RecordStatus = (int)ApprenticeshipStatus.Live,
                LocationType = LocationType.SubRegion,
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            };

            var locations = new List<ApprenticeshipLocation>
            {
                employer,
                classroom,
                both,
                archived,
                region,
                subRegion,
            };

            var expectedLocations = new List<ApprenticeshipLocation>
            {
                // employer,
                classroom,
                both,
                // archived,
                // region,
                // subRegion,
            };

            // Act
            var relevantApprenticeshipLocations = VenueAnalyser.RelevantApprenticeshipLocations(locations);

            // Assert
            relevantApprenticeshipLocations.Should().BeEquivalentTo(expectedLocations);
        }

        private static ApprenticeshipLocationAddress AddressFromVenue(Venue venue)
        {
            return new ApprenticeshipLocationAddress
            {
                Address1 = venue.AddressLine1,
                Address2 = venue.AddressLine2,
                Town = venue.Town,
                County = venue.County,
                Postcode = venue.Postcode,
                Latitude = venue.Latitude,
                Longitude = venue.Longitude,
                Phone = venue.PHONE,
                Email = venue.Email,
                Website = venue.Website,
            };
        }

    }
}
